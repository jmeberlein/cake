// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Cake.Core.Graph;
using Xunit;

namespace Cake.Core.Tests.Unit.Graph
{
    public sealed class CakeGraphBuilderTests
    {
        public sealed class TheBuildMethod
        {
            [Fact]
            public void Should_Add_All_Tasks_As_Nodes_In_Graph()
            {
                // Given, When
                var tasks = new List<CakeTask> { new ActionTask("A"), new ActionTask("B") };
                var graph = CakeGraphBuilder.Build(tasks);

                // Then
                Assert.Equal(2, graph.Nodes.Count);
            }

            [Fact]
            public void Should_Create_Edges_Between_Dependencies()
            {
                // Given
                var task1 = new ActionTask("A");
                var task2 = new ActionTask("B");
                task2.AddDependency("A");

                var tasks = new List<CakeTask>
                {
                    task1, task2
                };
                var graph = CakeGraphBuilder.Build(tasks);

                // Then
                Assert.Equal("B", graph.Nodes[0].DownIn[0].Name);
                Assert.Equal("A", graph.Nodes[1].DownOut[0].Name);
            }

            [Fact]
            public void Should_Create_Edges_Between_Reversed_Dependencies()
            {
                CakeGraph graph = null;

                // Given
                var task1 = new ActionTask("A");
                var task2 = new ActionTask("B");
                task2.AddReverseDependency("A");

                graph = CakeGraphBuilder.Build(new List<CakeTask>
                {
                    task1, task2
                });

                // Then
                Assert.Equal("B", graph.Nodes[0].DownOut[0].Name);
                Assert.Equal("A", graph.Nodes[1].DownIn[0].Name);
            }

            [Fact]
            public void Should_Throw_When_Depending_On_Task_That_Does_Not_Exist()
            {
                // Given
                var task = new ActionTask("A");
                task.AddDependency("C");
                var tasks = new List<CakeTask> { task };

                // When
                var result = Record.Exception(() => CakeGraphBuilder.Build(tasks));

                // Then
                Assert.NotNull(result);
                Assert.Equal("Task 'A' is dependent on task 'C' which does not exist.", result.Message);
            }

            [Fact]
            public void Should_Not_Throw_When_Depending_On_Optional_Task_That_Does_Not_Exist()
            {
                // Given
                var task = new ActionTask("A");
                task.AddDependency("C", false);
                var tasks = new List<CakeTask> { task };

                // When
                var result = Record.Exception(() => CakeGraphBuilder.Build(tasks));

                // Then
                Assert.Null(result);
            }

            [Fact]
            public void Should_Throw_When_Reverse_Dependency_Is_Depending_On_Task_That_Does_Not_Exist()
            {
                // Given
                var task = new ActionTask("A");
                task.AddReverseDependency("C");
                var tasks = new List<CakeTask> { task };

                // When
                var result = Record.Exception(() => CakeGraphBuilder.Build(tasks));

                // Then
                Assert.NotNull(result);
                Assert.Equal("Task 'A' has specified that it's a dependency for task 'C' which does not exist.", result.Message);
            }

            [Fact]
            public void Should_Not_Throw_When_An_Reverse_Dependency_Is_Depending_On_An_Optional_Task_That_Does_Not_Exist()
            {
                // Given
                var task = new ActionTask("A");
                task.AddReverseDependency("C", required: false);
                var tasks = new List<CakeTask> { task };

                // When
                var result = Record.Exception(() => CakeGraphBuilder.Build(tasks));

                // Then
                Assert.Null(result);
            }

            [Fact]
            public void Should_Throw_When_A_Circular_Dependency_Exists()
            {
                // Given
                var task1 = new ActionTask("A");
                var task2 = new ActionTask("B");
                var task3 = new ActionTask("C");
                task2.AddDependency("A");
                task3.AddDependency("B");
                task3.AddDependency("A", predependency: false);

                var tasks = new List<CakeTask>
                {
                    task1, task2, task3
                };

                // When
                var result = Record.Exception(() => CakeGraphBuilder.Build(tasks));

                // Then
                Assert.NotNull(result);
                Assert.Equal("Graph contains cyclic dependencies", result.Message);
            }
        }
    }
}