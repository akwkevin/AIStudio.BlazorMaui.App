﻿using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace AIStudio.BlazorDiagram.Models
{
    public class Table : NodeModel
    {
        public Table(Point position = null) : base(position, RenderLayer.HTML)
        {
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "Id",
                    Type = ColumnType.Integer,
                    Primary = true
                },
                new Column
                {
                    Name = "Test",
                    Type = ColumnType.Integer
                }
            };

            AddPort(Columns[0], PortAlignment.Right);
            AddPort(Columns[1], PortAlignment.Left);
        }

        public Table(string id, Point position = null) : base(id, position, RenderLayer.HTML)
        {
            Columns = new List<Column>();
        }

        public string Name { get; set; } = "Table";
        public List<Column> Columns { get; }
        public bool HasPrimaryColumn => Columns.Any(c => c.Primary);

        public ColumnPort GetPort(Column column) => Ports.Cast<ColumnPort>().FirstOrDefault(p => p.Column == column);

        public void AddPort(Column column, PortAlignment alignment) => AddPort(new ColumnPort(this, column, alignment));

        public void RemoveColumn(Column column)
        {
            RemovePort(GetPort(column));
            Columns.Remove(column);
        }

        public void AddColumn(Column column)
        {
            Columns.Add(column);
            AddPort(column, column.Primary ? PortAlignment.Right : PortAlignment.Left);
        }
    }
}
