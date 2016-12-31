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
