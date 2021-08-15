module App

open Elmish
open Elmish.React
open Feliz

let inline (%!) a b = (a % b + b) % b

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
let grid = [for i in 0 .. 4 do for j in 0 .. 4 -> [i; j]]
let keys = List.map (fun (tup: list<int>) -> tup |> (List.map (sprintf "%i")) |> String.concat "") grid
let initialColors = Map(seq {for key in keys -> (key, {backgroundColor = ""; clicked = false; count = 0; colorIndex = -1; index = -1})})

let init() =
    { 
      Count = 0 
      counter = 1
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
        let currClicked = state.squareColors.[key].clicked
        let currCount = state.squareColors.[key].count
        if currClicked then currCount else state.counter
        |> (fun newCount -> {state.squareColors.[key] with count = newCount; clicked = true})
        |> (fun newKeyedSquareColors -> state.squareColors.Add(key, newKeyedSquareColors))
        |> (Map.map (fun _ value -> 
          if value.clicked then // update colors for clicked squares
            (value.colorIndex + value.count) %! colors.Length
            |> (fun newIndex -> if newIndex = value.colorIndex then (newIndex + 1) %! colors.Length else newIndex)
            |> (fun newIndex -> { value with backgroundColor = colors.[newIndex]; colorIndex = newIndex })
          else 
            value ))
        |> (fun sc -> { state with squareColors = sc; counter = state.counter + (if currClicked then 0 else 1)})

// square stuff
type SquareProps =
  {
    backgroundColor: string
    xPos: int
    yPos: int
    changeColor: Browser.Types.MouseEvent -> unit
    transitionDuration: int
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
      style.transitionDuration (System.TimeSpan(0, 0, 0, 0, props.transitionDuration))
      style.transitionProperty "background-color"
      style.transitionTimingFunction.easeInOut
      // transition here
    ]
    prop.onClick props.changeColor
    prop.text props.text
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
                transitionDuration = 500
                text = key
              }
            | _  -> 
              Square {
                backgroundColor = state.squareColors.["00"].backgroundColor
                xPos = 0
                yPos = 0
                changeColor = (fun _ -> printfn "oops")
                transitionDuration = 500
                text = "00"
              }
        ]
      ]
    ]
  ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run