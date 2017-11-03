// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Cake.Core.Graph;
using Xunit;

namespace Cake.Core.Tests.Unit.Graph
{
    public sealed class CakeGraphTests
    {
        public sealed class TheAddMethod
        {
            [Fact]
            public void Should_Throw_If_Provided_Node_Is_Null()
            {
                // Given
                var graph = new CakeGraph();

                // When
                var result = Record.Exception(() => graph.Add(null));

                // Then
                AssertEx.IsArgumentNullException(result, "node");
            }

            [Fact]
            public void Should_Add_Node_To_Graph()
            {
                // Given
                var graph = new CakeGraph();

                // When
                graph.Add("start");

                // Then
                Assert.Equal(1, graph.Nodes.Count);
            }

            [Fact]
            public void Should_Throw_If_Node_Already_Is_Present_In_Graph()
            {
                // Given
                var graph = new CakeGraph();
                graph.Add("start");

                // When
                var result = Record.Exception(() => graph.Add("start"));

                // Then
                Assert.IsType<CakeException>(result);
                Assert.Equal("Node has already been added to graph.", result?.Message);
            }
        }

        public sealed class TheConnectMethod
        {
            [Fact]
            public void Should_Create_Edge_Between_Connected_Nodes()
            {
                // Given
                var graph = new CakeGraph();
                graph.Add("start");
                graph.Add("end");

                // When
                graph.Connect("start", "end");

                // Then
                Assert.Equal("end", graph.Nodes[0].DownOut[0].Name);
                Assert.Equal("start", graph.Nodes[1].DownIn[0].Name);
            }

            [Fact]
            public void Should_Add_Start_Node_If_Missing_To_Node_Collection()
            {
                // Given
                var graph = new CakeGraph();
                graph.Add("end");

                // When
                graph.Connect("start", "end");

                // Then
                Assert.Equal(2, graph.Nodes.Count);
            }

            [Fact]
            public void Should_Add_End_Node_If_Missing_To_Node_Collection()
            {
                // Given
                var graph = new CakeGraph();
                graph.Add("start");

                // When
                graph.Connect("start", "end");

                // Then
                Assert.Equal(2, graph.Nodes.Count);
            }

            [Fact]
            public void Should_Throw_If_Edge_Is_Reflexive()
            {
                // Given
                var graph = new CakeGraph();

                // When
                var result = Record.Exception(() => graph.Connect("start", "start"));

                // Then
                Assert.IsType<CakeException>(result);
                Assert.Equal("Reflexive edges in graph are not allowed.", result?.Message);
            }
        }

        public sealed class TheExistMethod
        {
            [Fact]
            public void Should_Find_Node_In_Graph()
            {
                // Given
                var graph = new CakeGraph();
                graph.Add("start");

                // When, Then
                Assert.True(graph.Exist("start"));
            }

            [Fact]
            public void Should_Find_Node_In_Graph_Regardless_Of_Casing()
            {
                // Given
                var graph = new CakeGraph();
                graph.Add("start");

                // When, Then
                Assert.True(graph.Exist("START"));
            }

            [Fact]
            public void Should_Not_Find_Non_Existing_Node_In_Graph()
            {
                // Given
                var graph = new CakeGraph();
                graph.Add("start");

                // When, Then
                Assert.False(graph.Exist("other"));
            }
        }

        public sealed class TheTraverseMethod
        {
            [Fact]
            public void Should_Return_Empty_Collection_Of_Nodes_If_Target_Was_Not_Found()
            {
                // Given
                var graph = new CakeGraph();
                graph.Connect("A", "B");
                graph.Connect("C", "D");
                graph.Connect("B", "C");

                // When
                var result = graph.Traverse("E").ToArray();

                // Then
                Assert.Empty(result);
            }

            [Fact]
            public void Should_Traverse_Graph_In_Correct_Order()
            {
                // Given
                var graph = new CakeGraph();
                graph.Connect("A", "B");
                graph.Connect("C", "D");
                graph.Connect("B", "C");

                // When
                var result = graph.Traverse("A").ToArray();

                // Then
                Assert.Equal(4, result.Length);
                Assert.Equal("D", result[0]);
                Assert.Equal("C", result[1]);
                Assert.Equal("B", result[2]);
                Assert.Equal("A", result[3]);
            }

            [Fact]
            public void Should_Traverse_Graph_In_Correct_Order_Regardless_Of_Casing_Of_Root()
            {
                // Given
                var graph = new CakeGraph();
                graph.Connect("A", "B");
                graph.Connect("C", "D");
                graph.Connect("B", "C");

                // When
                var result = graph.Traverse("a").ToArray();

                // Then
                Assert.Equal(4, result.Length);
                Assert.Equal("D", result[0]);
                Assert.Equal("C", result[1]);
                Assert.Equal("B", result[2]);
                Assert.Equal("A", result[3]);
            }

            [Fact]
            public void Should_Skip_Nodes_That_Are_Not_On_The_Way_To_The_Target()
            {
                // Given
                var graph = new CakeGraph();
                graph.Connect("A", "B");
                graph.Connect("C", "E");
                graph.Connect("B", "D");
                graph.Connect("D", "E");

                // When
                var result = graph.Traverse("A").ToArray();

                // Then
                Assert.Equal(4, result.Length);
                Assert.Equal("E", result[0]);
                Assert.Equal("D", result[1]);
                Assert.Equal("B", result[2]);
                Assert.Equal("A", result[3]);
            }

            [Fact]
            public void Should_Throw_If_Encountering_Circular_Reference()
            {
                var graph = new CakeGraph();
                graph.Connect("A", "B");
                graph.Connect("B", "C");
                graph.Connect("C", "A");

                Assert.Equal(true, graph.ContainsCycles());
            }
        }
    }
}