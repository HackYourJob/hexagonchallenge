module AisStorageShould
    
    open Xunit
    open FsUnit.Xunit
    open HexagonRestApi.AisService
    open HexagonRestApi.Domain.Domain

    
    let usingInMemoryStorage = {
        GetAll = AiStorageInmemory.GetAll
        Exists = AiStorageInmemory.Exists
        Add = AiStorageInmemory.Add
        Update = AiStorageInmemory.Update
        GetById = AiStorageInmemory.GetById
        }
    
    let private buildAiId ai=
        String.concat "." [ai.UserId; ai.Password; ai.AiName;]

    [<Fact>]
    let ``create ai when submit an unknown ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
      
        AisService.submitAi usingInMemoryStorage aiToSubmit|> should equal aiToSubmit 
        usingInMemoryStorage.GetById(buildAiId aiToSubmit)|> should equal aiToSubmit 
        

    [<Fact>] 
    let ``update ai when submit a known ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
        let aiUpdated = {AiName="test";UserId="test";Password="test";Content="testUpdated"}
        
        usingInMemoryStorage.Add(buildAiId(aiToSubmit), aiToSubmit) |> ignore
        
        AisService.submitAi usingInMemoryStorage aiUpdated |> should equal aiUpdated
        usingInMemoryStorage.GetById(buildAiId aiToSubmit)|> should equal aiUpdated         

    [<Fact>] 
    let ``return none when unknow ai`` () =              
        AisService.getAi usingInMemoryStorage ("unknow","unknow","unknow") |> should equal None

