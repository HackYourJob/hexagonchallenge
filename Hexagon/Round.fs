module Hexagon.Round

open Domain

module TransactionValidation =
    type private TransactionEither =
        | Valid of TransactionEitherContext
        | Invalid of InvalidReason
    and TransactionEitherContext = { Transaction: TransactionParameters; FromCell: Cell; ToCell: Cell; AiId: AiId }
    and InvalidReason =
        | NotOwnCell
        | NotEnoughResources
        | InvalidMove

    let private bind f = function | Invalid p -> Invalid p | Valid p -> f p

    let private createContext (getCell: CellId -> Cell option) (aiId: AiId) (transaction: TransactionParameters) =
        match getCell transaction.FromId, getCell transaction.ToId with
        | Some fromCell, Some toCell -> Valid { FromCell = fromCell; ToCell = toCell; Transaction = transaction; AiId = aiId }
        | _ -> Invalid NotOwnCell

    let private validTransaction (isNeighboursOf: CellId -> CellId -> bool) (context: TransactionEither) =
        let isOwnCell context =
            match context.FromCell.State with
            | Own param when param.AiId = context.AiId -> Valid context
            | _ -> Invalid NotOwnCell

        let haveEnoughResources context =
            match extractResources context.FromCell with
            | r when r >= context.Transaction.AmountToTransfert -> Valid context
            | _ -> Invalid NotEnoughResources

        let isNeighbours context =
            if isNeighboursOf context.Transaction.FromId context.Transaction.ToId
            then Valid context
            else Invalid InvalidMove
    
        context
        |> bind isOwnCell
        |> bind haveEnoughResources
        |> bind isNeighbours

    let validAiAction getCell isNeighboursOf aiId (transaction: AiActions) =
        match transaction with
        | Sleep
        | Bug _ -> transaction
        | Transaction t -> 
            match createContext getCell aiId t |> validTransaction isNeighboursOf with
            | Invalid e -> sprintf "%A" e |> Bug
            | Valid c -> transaction

module BoardHandler =
    type private Comparison = More | Less | Same

    let private compare a b =
        if a < b then Less
        else if a = b then Same
        else More

    let convertToBoardEvent fromCell toCell amountToTransfert : GameEvents option =
        match fromCell.State with
        | Free _ -> Option.None
        | Own { AiId = fromAiId; Resources = fromResources } ->
            let createCellChanged newFromResources newToResources =
                [
                    ResourcesChanged { CellId = fromCell.Id; Resources = newFromResources }
                    ResourcesChanged { CellId = toCell.Id; Resources = newToResources }
                ]
        
            let toResources = extractResources toCell
            match toCell.State, toResources |> compare amountToTransfert with
            | Own param, _ when param.AiId = fromAiId -> 
                ResourcesTransfered { FromId = fromCell.Id; ToId = toCell.Id; AmountToTransfert = amountToTransfert }, 
                createCellChanged (fromResources - amountToTransfert) (toResources + amountToTransfert)
            | _, Same -> 
                FightDrawed { FromId = fromCell.Id; ToId = toCell.Id },
                createCellChanged 0 (toResources + 1)
            | _, More ->
                FightWon { FromId = fromCell.Id; ToId = toCell.Id; AmountToTransfert = amountToTransfert; AiId = fromAiId },
                [ResourcesChanged { CellId = fromCell.Id; Resources = fromResources - amountToTransfert }; Owned { CellId = toCell.Id; Resources = amountToTransfert - toResources; AiId = fromAiId }]
            | _, Less -> 
                FightLost { FromId = fromCell.Id; ToId = toCell.Id; AmountToTransfert = amountToTransfert }, 
                createCellChanged (fromResources - amountToTransfert) (toResources - amountToTransfert)
            |> Board |> Some

    let generateEvents getCell aiAction =
        match aiAction with
        | Transaction param -> 
            let fromCell = getCell param.FromId
            let toCell = getCell param.ToId

            convertToBoardEvent fromCell toCell param.AmountToTransfert
            |> Option.toList
        | Bug _
        | Sleep -> []

    let generateResourcesIncreased ownCells =
        ownCells
        |> Seq.filter (fun (_, resources) -> resources < 100)
        |> Seq.map (fun (id, resources) -> ResourcesChanged { CellId = id; Resources = resources + 1 }) 
        |> Seq.toList
        |> fun cellsChanged -> ResourcesIncreased 1, cellsChanged
        |> Board

module AiActions =
    let private apply getCell action =
        seq {
            yield AiPlayed action
            yield! BoardHandler.generateEvents getCell action
        }

    let playAi getCellsWithNeighboursOf getCell validAction (aiId: AiId, play: AiPlayParameters -> AiActions) =
        getCellsWithNeighboursOf aiId
        |> play
        |> validAction aiId
        |> apply getCell

let playAis (play: (AiId * (AiPlayParameters -> AiActions)) -> GameEvents seq) ais =
    ais 
    |> Seq.map play

let createPlayAi getCellsWithNeighboursOf getCell isNeighboursOf =
    let validAction = TransactionValidation.validAiAction getCell isNeighboursOf
    AiActions.playAi getCellsWithNeighboursOf (getCell >> Option.get) validAction

let runRound getAllOwnCells playAi ais = 
    seq {
        yield! ais |> playAis playAi |> Seq.collect id
            
        yield getAllOwnCells () |> BoardHandler.generateResourcesIncreased
    }

let createRunRound getCellsWithNeighboursOf getCell isNeighboursOf getAllOwnCells =
    createPlayAi getCellsWithNeighboursOf getCell isNeighboursOf
    |> runRound getAllOwnCells