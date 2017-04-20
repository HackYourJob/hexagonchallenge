module AisStorageShould
    
    open Xunit
    open FsUnit.Xunit
    open HexagonRestApi
    open HexagonRestApi.Domain
        
    let private buildAiId ai=
        String.concat "." [ai.UserId; ai.Password; ai.AiName;]

    [<Fact>]
    let ``create ai when submit an unknown ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
      
        AisService.submitAi AiStorageInmemory.updateOrAdd aiToSubmit|> should equal aiToSubmit 
        AiStorageInmemory.GetById(buildAiId aiToSubmit)|> should equal aiToSubmit 
        

    [<Fact>] 
    let ``update ai when submit a known ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
        let aiUpdated = {AiName="test";UserId="test";Password="test";Content="testUpdated"}
        
        AiStorageInmemory.Add(buildAiId(aiToSubmit), aiToSubmit) |> ignore
        
        AisService.submitAi AiStorageInmemory.updateOrAdd aiUpdated |> should equal aiUpdated
        AiStorageInmemory.GetById(buildAiId aiToSubmit)|> should equal aiUpdated         

    [<Fact>] 
    let ``return none when unknow ai`` () =              
        AisService.getAi AiStorageInmemory.tryToGetCode ({Ai.AiName="unknow";UserId="unknow";Password="unknow";Content="unknow"}) |> should equal None

