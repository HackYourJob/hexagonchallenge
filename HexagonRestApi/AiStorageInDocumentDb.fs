module AiStorageInDocumentDb

open System
open HexagonRestApi.Domain.Domain
open Microsoft.Azure.Documents
open Microsoft.Azure.Documents.Client
open Microsoft.Azure.Documents.Linq
open FSharp.Interop.Dynamic

type AiForStorage = {
  AiName : string
  UserId : string
  Password : string
  Content : string
  id : string
}

let endpointUrl = "https://hexagon.documents.azure.com:443/"
let authKey = "2HiVq0rAGNLUNZ5om5nmajikzM7QD6WzRjQyGSWQdPC8vdPPi5qVWQgUCqWZ12MHMMf0ysjLqNs4xYStdDQM3Q=="
let DatabaseId = "Ais"
let CollectionId = "AiCollection"

let client = new DocumentClient(new Uri(endpointUrl), authKey) 

let private buildAiId (ai: Ai)=
   String.concat "." [ai.UserId; ai.Password; ai.AiName;]

let documentToAi (document : ResourceResponse<Document>) =
    {AiName = document.Resource?AiName; UserId = document.Resource?UserId; Password = document.Resource?Password; Content = document.Resource?Content}

let GetById id = 
    let document = client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id)).Result
    documentToAi document

let GetAll () = 
    let feedOption = new FeedOptions() 
    let documents  = client.CreateDocumentQuery<Document>(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), feedOption)
    documents |> Seq.map (fun doc -> GetById doc.Id)

let Update (id,ai) = 
    let document = client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), ai).Result
    ai

let Exists id = 
    let feedOption = new FeedOptions() 
    feedOption.MaxItemCount <- Nullable<int> 1 
    let documents = client.CreateDocumentQuery<Document>(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), feedOption)
    let existingDocument = documents |> Seq.filter (fun doc -> doc.Id = id) |> Seq.toList
    existingDocument.Length > 0

let Add (id,ai : Ai) = 
    let aiForStorage =  {AiName = ai.AiName; UserId = ai.UserId; Password = ai.Password; Content = ai.Content; id = buildAiId(ai)}
    let document =  client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), aiForStorage).Result
    ai
