using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Model.ApiResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecetaItemViewModel : ObservableObject
    {
        public RecetaResponse Receta { get; }

        [ObservableProperty]
        private bool isSelected;

        public string Nombre => Receta.Nombre;
        public int Id => Receta.IdReceta;

        public RecetaItemViewModel(RecetaResponse receta)
        {
            Receta = receta;
        }
    }
}