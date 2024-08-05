﻿namespace Sigma.NET

open System.Runtime.InteropServices
open System
open DynamicObj

// https://www.bsimard.com/2018/04/25/graph-viz-with-sigmajs.html


[<AutoOpen>]
type VisGraphElement() =
/// Creates a node with the specified key 
    static member node key = Node.Init(key = key) 
/// Configures node data with various optional parameters
    static member withNodeData(
            ?Label      : string,
            ?Size       : #IConvertible,
            ?Color      : string,
            ?Hidden     : bool,
            ?ForceLabel : bool,
            ?ZIndex     : int,
            ?StyleType  : StyleParam.NodeType,
            ?X          : #IConvertible,
            ?Y          : #IConvertible
    ) =
        fun (node:Node) -> 
            let styleType = Option.map StyleParam.NodeType.toString StyleType
            let displayData = 
                    match node.TryGetTypedValue<DisplayData>("attributes") with
                    | None -> DisplayData()
                    | Some a -> a
                    |> DisplayData.Style
                        (?Label=Label,?Size=Size,?Color=Color,?Hidden=Hidden,
                         ?ForceLabel=ForceLabel,?ZIndex=ZIndex,?StyleType=styleType,?X=X,?Y=Y)
            displayData |> DynObj.setValue node "attributes"
            node
    /// Creates an edge between the source and target nodes
    static member edge source target = Edge.Init(source = source,target = target) 
    /// Configures edge data with various optional parameters
    static member withEdgeData(
            ?Label      : string,
            ?Size       : #IConvertible,
            ?Color      : string,
            ?Hidden     : bool,
            ?ForceLabel : bool,
            ?ZIndex     : int,
            ?StyleType  : StyleParam.EdgeType,
            ?X          : #IConvertible,
            ?Y          : #IConvertible
    ) =
        fun (edge:Edge) -> 
            let styleType = Option.map StyleParam.EdgeType.toString StyleType
            let displayData = 
                    match edge.TryGetTypedValue<DisplayData>("attributes") with
                    | None -> DisplayData()
                    | Some a -> a
                    |> DisplayData.Style
                        (?Label=Label,?Size=Size,?Color=Color,?Hidden=Hidden,
                         ?ForceLabel=ForceLabel,?ZIndex=ZIndex,?StyleType=styleType,?X=X,?Y=Y)
            displayData |> DynObj.setValue edge "attributes"
            edge


// Module to manipulate and sytely a graph
type VisGraph() =
    ///creates an empty graph
    [<CompiledName("Empty")>]
    static member empty () = SigmaGraph()

    /// Adds a single node to a graph
    [<CompiledName("WithNode")>]
    static member withNode (node:Node) (graph:SigmaGraph) = 
        graph.AddNode(node)
        graph       
    /// Adds a sequence of nodes to a graph
    [<CompiledName("WithNodes")>]
    static member withNodes (nodes:Node seq) (graph:SigmaGraph) = 
        nodes |> Seq.iter (fun node -> graph.AddNode node) 
        graph
    ///Adds a single edge to a graph
    [<CompiledName("WithEdge")>]
    static member withEdge (edge:Edge) (graph:SigmaGraph) = 
        graph.AddEdge(edge)
        graph       
    ///Adds a sequence of edges to a graph
    [<CompiledName("WithEdges")>]
    static member withEdges (edges:Edge seq) (graph:SigmaGraph) = 
        edges |> Seq.iter (fun edge -> graph.AddEdge edge) 
        graph
    ///gives a graph a random layout
    [<CompiledName("WithRandomLayout")>] 
    static member withRandomLayout(
        [<Optional; DefaultParameterValue(null)>] ?Scale,
        [<Optional; DefaultParameterValue(null)>] ?Center,
        [<Optional; DefaultParameterValue(null)>] ?Dimensions    
        ) = 
            fun (graph:SigmaGraph) -> 
                graph.Layout <- Layout.Random (RandomOptions.Init(?Dimensions=Dimensions,?Center=Center,?Scale=Scale))
                graph   
    ///gives a graph a circular layout
    [<CompiledName("WithCircularLayout")>]
    static member withCircularLayout(
        [<Optional; DefaultParameterValue(null)>] ?Scale,
        [<Optional; DefaultParameterValue(null)>] ?Center
        ) = 
            fun (graph:SigmaGraph) -> 
                graph.Layout <- Layout.Circular (CircularOptions.Init(?Scale=Scale,?Center=Center))
                graph   
 


     /// Applies the ForceAtlas2 layout algorithm to a graph
    [<CompiledName("WithForceAtlas2")>]
    static member withForceAtlas2(
        [<Optional; DefaultParameterValue(null)>] ?Iterations, 
        [<Optional; DefaultParameterValue(null)>] ?Settings,
        [<Optional; DefaultParameterValue(null)>] ?GetEdgeWeight) = 
        fun (graph:SigmaGraph) ->
            graph.Layout <- Layout.FA2 (FA2Options.Init(?Iterations=Iterations,?GetEdgeWeight=GetEdgeWeight,?Settings=Settings))
            graph
    /// Applies the Noverlap layout algorithm to a graph
    [<CompiledName("WithNoverlap")>]
    static member withNoverlap(
        [<Optional; DefaultParameterValue(null)>] ?MaxIterations,
        [<Optional; DefaultParameterValue(null)>] ?Settings) = 
        fun (graph:SigmaGraph) ->
            graph.Layout <- Layout.Noverlap (NoverlapOptions.Init(?MaxIterations = MaxIterations,?Settings=Settings))
            graph



    /// Sets the size of a canvas (in pixels)
    [<CompiledName("WithSize")>]
    static member withSize
        (
            [<Optional; DefaultParameterValue(null)>] ?Width: CssLength,
            [<Optional; DefaultParameterValue(null)>] ?Height: CssLength
        ) =

        fun (graph:SigmaGraph) ->
            graph.Width <- Option.defaultValue Defaults.DefaultWidth Width
            graph.Height <- Option.defaultValue Defaults.DefaultHeight Height
            
            graph

    /// Sets the Renderer settings
    [<CompiledName("WithRenderer")>]
    static member withRenderer(settings:Render.Settings) = 
        fun (graph:SigmaGraph) ->
            graph.Settings <- settings
            graph
    ///Hoverselector lets you hover over a node and see all its edges
    [<CompiledName("WithRenderer")>]
    static member withHoverSelector(?enable:bool) = 
        fun (graph:SigmaGraph) ->
            let enable = Option.defaultValue true enable
            if enable then
                graph.Widgets.Add("""const state={};function setHoveredNode(e){e?(state.hoveredNode=e,state.hoveredNeighbors=new Set(graph.neighbors(e))):(state.hoveredNode=void 0,state.hoveredNeighbors=void 0),renderer.refresh()}renderer.on("enterNode",({node:e})=>{setHoveredNode(e)}),renderer.on("leaveNode",()=>{setHoveredNode(void 0)}),renderer.setSetting("nodeReducer",(e,t)=>{let o=t;return state.hoveredNeighbors&&!state.hoveredNeighbors.has(e)&&state.hoveredNode!==e&&(o.label="",o.color="#f6f6f6"),o}),renderer.setSetting("edgeReducer",(e,t)=>{let o=t;return state.hoveredNode&&!graph.hasExtremity(e,state.hoveredNode)&&(o.hidden=!0),o});""")            
            graph
    ///Shows a graph as HTML
    [<CompiledName("Show")>] 
    static member show() (graph:SigmaGraph) = 
        HTML.show(graph)