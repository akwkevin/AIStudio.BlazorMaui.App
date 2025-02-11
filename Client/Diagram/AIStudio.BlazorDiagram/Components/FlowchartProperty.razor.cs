﻿using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using AIStudio.BlazorDiagram.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using AntDesign;
using AIStudio.Util.Common;

namespace AIStudio.BlazorDiagram.Components
{
    public partial class FlowchartProperty : IDisposable
    {
        private FlowchartNodelModel Model;
        private LinkLabelModel LinkLabelModel;

        [CascadingParameter]
        public Diagram Diagram { get; set; }

        [Parameter]
        public List<SelectOption> Users { get; set; } = new List<SelectOption>();

        [Parameter]
        public List<SelectOption> Roles { get; set; } = new List<SelectOption>();

        public void Dispose()
        {
            Diagram.SelectionChanged -= Diagram_SelectionChanged;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Diagram.SelectionChanged += Diagram_SelectionChanged;
        }

        private void Diagram_SelectionChanged(SelectableModel model)
        {
            if (model is FlowchartNodelModel flowchartNodelModel)
            {
                Model = model.Selected ? flowchartNodelModel : null;
                StateHasChanged();
            }
            else if(model is Blazor.Diagrams.Core.Models.LinkModel linkmodel)
            {
                LinkLabelModel = linkmodel.Selected ? linkmodel.Labels.FirstOrDefault() : null;
                StateHasChanged();
            }
        }

        private void OnTitleChanged(ChangeEventArgs e)
        {
            if (Model == null)
                return;

            Model.Title = e.Value.ToString();
            Model.Refresh();
        }

        private void OnContentChanged(ChangeEventArgs e)
        {
            
            if (LinkLabelModel == null)
                return;

            LinkLabelModel.Content = e.Value.ToString();
            LinkLabelModel.Parent.Refresh();
            //编辑不成功，待续
        
        }

        private void OnColorChanged(ChangeEventArgs e)
        {
            if (Model == null)
                return;

            Model.Color = e.Value.ToString();
            Model.Refresh();
        }

        private void OnActTypeChanged(string value)
        {
            if (Model == null)
                return;

            Model.ActType = value;
            Model.Refresh();
        }
    }
}
