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

using System;
using System.Collections.Generic;
using FluentAssertions;
using Neo4j.Driver.Internal.Types;
using Xunit;

namespace Neo4j.Driver.Internal.IO.ValueSerializers
{
    public class ElementPathSerializerTests : PackStreamSerializerTests
    {
        internal override IPackStreamSerializer SerializerUnderTest => new PathSerializer();

        internal override IEnumerable<IPackStreamSerializer> SerializersNeeded =>
            new IPackStreamSerializer[] { new ElementNodeSerializer(), new ElementUnboundRelationshipSerializer() };


        [Fact]
        public void ShouldDeserializeAddingElementIds()
        {
            var writerMachine = CreateWriterMachine();
            var writer = writerMachine.Writer();

            SerializeElementPath(writer,
                new List<Node>
                {
                    new Node(1, new List<string>{"a"}, new Dictionary<string, object>()),
                    new Node(2, new List<string>{"a"}, new Dictionary<string, object>()),
                },
                new List<Relationship>
                {
                    new Relationship(1, -1, -1, "LIKES", new Dictionary<string, object>())
                },
                new List<int>
                {
                    1, 1
                });

            var readerMachine = CreateReaderMachine(writerMachine.GetOutput());
            var value = readerMachine.Reader().Read();

            var path = value.Should().BeOfType<Path>();

            path.Which.Nodes.Should().AllBeOfType<Node>();
            path.Which.Relationships.Should().AllBeOfType<Relationship>();

            var nodes = path.Which.Nodes;
            var relationships = path.Which.Relationships;


            nodes[0].Id.Should().Be(1L);
            nodes[0].ElementId.Should().Be("1");
            nodes[1].Id.Should().Be(2L);
            nodes[1].ElementId.Should().Be("2");

            relationships[0].Id.Should().Be(1);
            relationships[0].ElementId.Should().Be("1");
            relationships[0].StartNodeId.Should().Be(1L);
            relationships[0].StartNodeElementId.Should().Be("1");
            relationships[0].EndNodeId.Should().Be(2L);
            relationships[0].EndNodeElementId.Should().Be("2");
        }

        [Fact]
        public void ShouldDeserializeWithElementIds()
        {
            var writerMachine = CreateWriterMachine();
            var writer = writerMachine.Writer();

            SerializeElementPath(writer,
                new List<Node>
                {
                    new Node(1, "n1", new List<string>{"a"}, new Dictionary<string, object>()),
                    new Node(2, "n2",new List<string>{"a"}, new Dictionary<string, object>()),
                },
                new List<Relationship>
                {
                    new Relationship(1, "r1", -1, -1, "-1", "-1", "LIKES", new Dictionary<string, object>())
                },
                new List<int>
                {
                    1, 1
                });

            var readerMachine = CreateReaderMachine(writerMachine.GetOutput());
            var value = readerMachine.Reader().Read();

            var path = value.Should().BeOfType<Path>();

            path.Which.Nodes.Should().AllBeOfType<Node>();
            path.Which.Relationships.Should().AllBeOfType<Relationship>();

            var nodes = path.Which.Nodes;
            var relationships = path.Which.Relationships;

            nodes[0].Id.Should().Be(1L);
            nodes[0].ElementId.Should().Be("n1");
            nodes[1].Id.Should().Be(2L);
            nodes[1].ElementId.Should().Be("n2");

            relationships[0].Id.Should().Be(1);
            relationships[0].ElementId.Should().Be("r1");
            relationships[0].StartNodeId.Should().Be(1L);
            relationships[0].StartNodeElementId.Should().Be("n1");
            relationships[0].EndNodeId.Should().Be(2L);
            relationships[0].EndNodeElementId.Should().Be("n2");
        }

        private static void SerializeElementPath(IPackStreamWriter writer, List<Node> nodes, List<Relationship> rels,
            List<int> indicies)
        {
            writer.WriteStructHeader(3, PathSerializer.Path);
            writer.WriteListHeader(nodes.Count);

            foreach (var node in nodes)
            {
                writer.WriteStructHeader(3, ElementNodeSerializer.Node);

                if (node.Id == -1)
                    writer.WriteNull();
                else
                    writer.Write(node.Id);
                writer.Write(node.Labels);
                writer.Write(node.Properties);
                writer.Write(node.ElementId);
            }

            writer.WriteListHeader(rels.Count);

            foreach (var rel in rels)
            {
                writer.WriteStructHeader(3, UnboundRelationshipSerializer.UnboundRelationship);

                if (rel.Id == -1)
                    writer.WriteNull();
                else
                    writer.Write(rel.Id);
                writer.Write(rel.Type);
                writer.Write(rel.Properties);
                writer.Write(rel.ElementId);
            }

            writer.Write(indicies);
        }
    }
}