namespace HexagonRestApi.Domain

module Domain =

    type Ai = {
      UserId : string
      AiName : string
      Password : string
      Content : string
    }

    type Storage<'a> = {
        GetAll : unit -> 'a seq
        Exists : string -> bool
        Add : string*'a -> 'a
        Update : string*'a -> 'a
        GetById : string -> 'a
      }
