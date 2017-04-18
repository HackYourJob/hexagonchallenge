module AisStorageShould
    
    open Xunit
    open FsUnit.Xunit
    open HexagonRestApi.AisStorage

    [<Fact>]
    let ``create ai when submit an unknown ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
        AisStorage.submitAi aiToSubmit |> should equal aiToSubmit    
        
    [<Fact>] 
    let ``update ai when submit a known ai`` () =
        let aiToSubmit = {AiName="test";UserId="test";Password="test";Content="test"}
        let aiUpdated = {AiName="test";UserId="test";Password="test";Content="testUpdated"}
        AisStorage.submitAi aiToSubmit |> should equal aiToSubmit
        AisStorage.submitAi aiUpdated |> should equal aiUpdated
