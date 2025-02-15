﻿// Copyright (c) 2002-2022 "Neo4j,"
// Neo4j Sweden AB [http://neo4j.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Neo4j.Driver.Internal.Types;
using Xunit;

namespace Neo4j.Driver.Tests
{
    public class NodeTests
    {
        [Fact]
        public void ShouldEqualIfIdEquals()
        {
            var node1 = new Node(123, new[] { "buibui" }, null);
            var node2 = new Node(123, new[] { "lala" }, null);

            node1.Equals(node2).Should().BeTrue();
            Equals(node1, node2).Should().BeTrue();

            Dictionary<Node, int> nodes = new Dictionary<Node, int>();
            nodes.Add(node1, 123);

            nodes.TryGetValue(node2, out var value).Should().BeTrue();
            value.Should().Be(123);

            var node3Mock = new Mock<INode>();
            node3Mock.Setup(f => f.Id).Returns(123);
            node3Mock.Setup(f => f.ElementId).Returns("123");
            node3Mock.Setup(f => f.Labels)
                .Returns(new[]
                {
                    "same interface, different implementation"
                });
            var node3 = node3Mock.Object;

            node1.Equals(node3).Should().BeTrue();
            Equals(node1, node3).Should().BeTrue();
        }
    }
}