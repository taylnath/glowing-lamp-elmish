module App

open Elmish
open Elmish.React
open Feliz

type colorType = 
  {
    backgroundColor: string
    clicked: bool
    count: int
    colorIndex: int
    index: int // debug
  }

type State =
  {
    Count: int
    counter: int
    currentlyClicking: bool
    currentInterval: string
    clickingFast: bool
    squareColors: Map<string, colorType>
  }

type Msg =
    | Increment
    | Decrement
    | ChangeColorByKey of string

let colors = ["red"; "orange"; "yellow"; "green"; "blue"; "indigo"; "violet"]
let grid = [for i in 0 .. 5 do for j in 0 .. 5 -> [i; j]]
let keys = List.map (fun (tup: list<int>) -> tup |> (List.map (sprintf "%i")) |> String.concat "") grid
let initialColors = Map(seq {for key in keys -> (key, {backgroundColor = ""; clicked = false; count = 0; colorIndex = -1; index = -1})})

let init() =
    { 
      Count = 0 
      counter = 0
      currentlyClicking = false
      currentInterval = ""
      clickingFast = false
      squareColors = initialColors
    }

let GetColorTypeOrDefault x = 
  match x with 
  | Some y -> y
  | None -> {backgroundColor = ""; clicked = false; count = 0; colorIndex = -1; index = -1}

let update (msg: Msg) (state: State): State =
    match msg with
    | Increment ->
        { state with Count = state.Count + 1 }

    | Decrement ->
        { state with Count = state.Count - 1 }
    | ChangeColorByKey key -> 
        let newSquareColors = (
          if not state.squareColors.[key].clicked then
            state.squareColors.Add(key, {state.squareColors.[key] with count = state.counter; clicked = true})
          else 
            state.squareColors
        )
        newSquareColors
        |> (Map.map (fun key value -> 
          if value.clicked then
            let newColorIndexMaybe = (value.colorIndex + value.count) % colors.Length
            let newColorIndex = 
              (
                if newColorIndexMaybe = value.colorIndex then
                  (newColorIndexMaybe + 1) % colors.Length
                else
                  newColorIndexMaybe
              )
            let newColor = colors.[newColorIndex]
            { value with backgroundColor = newColor; colorIndex = newColorIndex }
          else 
            value
        ))
        |> (fun sc -> { state with squareColors = sc})
        // { state with squareColors = state.squareColors.Change("00", (fun x -> Some {GetColorTypeOrDefault x with backgroundColor = "red"}))}
    // | ChangeColorByKey key ->
    //   let mutable newSquareColors = state.squareColors
    //   if not state.squareColors.[key].clicked then 
    //     newSquareColors = {newSquareColors with newSquareColors.[key] = {newSquareColors.[key] with count = counter; clicked = true}}

// square stuff
type SquareProps =
  {
    backgroundColor: string
    xPos: int
    yPos: int
    changeColor: Browser.Types.MouseEvent -> unit
    text: string
  }


let Square (props : SquareProps) = 
  let side = 60
  //let transitionString = `background-color ${String(props.transitionTime)}s ease-in-out`;
  Html.div [
    prop.style [
      style.backgroundColor props.backgroundColor
      style.width (side - 22)
      style.height (side - 22)
      style.textAlign.center
      style.borderStyle.solid
      style.borderColor "black"
      style.borderWidth 2
      style.position.absolute
      style.left (props.xPos * 60)
      style.top (props.yPos * 60)
      style.padding 10
      // transition here
    ]
    prop.onClick props.changeColor
  ]
// end square

let render (state: State) (dispatch: Msg -> unit) =
  Html.div [
    prop.style [ style.margin 200; style.textAlign.center ]
    prop.children [
      Html.div [
        prop.style [
          style.position.absolute
          style.top 20
          style.left 20
        ]
        prop.children [
          for tup in grid do
            match tup with
            | [i; j] -> 
              let key = sprintf "%d%d" i j
              Square {
                backgroundColor = state.squareColors.[key].backgroundColor
                xPos = i
                yPos = j
                changeColor = (fun _ -> dispatch (ChangeColorByKey key))
                text = key
              }
            | _  -> 
              Square {
                backgroundColor = state.squareColors.["00"].backgroundColor
                xPos = 0
                yPos = 0
                changeColor = (fun _ -> printfn "oops")
                text = "00"
              }
        ]
      ]
    ]
  ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run