module App

open System
open Fable.Core
open Browser.Dom
open Fable.Reaction
open FSharp.Control
open Feliz

type AppState = {
    SavedInput : string option
}

type AppMessage =
    | SaveNewInput of string
    | UpperCase
    | LowerCase


let AppStream (state: AppState) (messages: IAsyncObservable<AppMessage>) =
    messages
    |> AsyncRx.tapOnNext (fun msg ->
        JS.console.debug("[Stream] State: ", state)
        JS.console.debug("[Stream] Message: ", msg)
    )
    |> AsyncRx.choose (function
        | SaveNewInput _ as msg -> Some msg
        | UpperCase
        | LowerCase as msg ->
            state.SavedInput
            |> Option.map (fun _ -> msg)
    ) |> AsyncRx.tag "AppStream"


let Update (state: AppState) (message: AppMessage) =
    JS.console.debug("[Update] State: ", state)
    JS.console.debug("[Update] Message: ", message)
    match message with
    | SaveNewInput input -> { state with SavedInput = Some input }
    | UpperCase ->
        { state with
            SavedInput = state.SavedInput |> Option.map (fun x -> x.ToUpperInvariant()) }
    | LowerCase ->
        { state with
            SavedInput = state.SavedInput |> Option.map (fun x -> x.ToLowerInvariant()) }


[<ReactComponent>]
let AppView (state: AppState) (dispatch: Dispatch<AppMessage>) =
    let input, setInput = React.useState ""
    let savedInput = React.useMemo(fun () ->
        defaultArg state.SavedInput ""
    , [| state.SavedInput |])
    Html.div [
        prop.style [style.display.flex; style.alignItems.center; style.justifyContent.center]
        prop.children [
            Html.div [
                prop.style [style.display.flex; style.flexDirection.column; style.alignItems.center; style.justifyContent.center]
                prop.children [
                    Html.div [
                        Html.input [
                            prop.value input
                            prop.onChange setInput
                        ]
                        Html.button [
                            prop.onClick (fun _ -> SaveNewInput input |> dispatch)
                            prop.text "Save"
                        ]
                    ]
                    Html.div [
                        Html.p [
                            prop.text $"Saved Input: {savedInput}"
                        ]
                    ]
                    Html.div [
                        Html.button [
                            prop.text "To Upper"
                            prop.onClick (fun _ -> dispatch UpperCase)
                        ]
                        Html.button [
                            prop.text "To Lower"
                            prop.onClick (fun _ -> dispatch LowerCase)
                        ]
                    ]
                ]
            ]
        ]
    ]


[<ReactComponent>]
let App () =
    Reaction.streamComponent({ SavedInput = None }, AppView, Update, AppStream)


ReactDOM.render(App(), document.getElementById "app")
