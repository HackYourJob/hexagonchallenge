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

    [<Fact>]
    let ``create ai when submit an unknown ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
        AisService.submitAi usingInMemoryStorage aiToSubmit |> should equal aiToSubmit    
        
    [<Fact>] 
    let ``update ai when submit a known ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
        let aiUpdated = {AiName="test";UserId="test";Password="test";Content="testUpdated"}
        AisService.submitAi usingInMemoryStorage aiToSubmit |> should equal aiToSubmit
        AisService.submitAi usingInMemoryStorage aiUpdated |> should equal aiUpdated
