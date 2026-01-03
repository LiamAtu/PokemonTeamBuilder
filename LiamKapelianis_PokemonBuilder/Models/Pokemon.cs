using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.ComponentModel;

namespace LiamKapelianis_PokemonBuilder.Models
{
    public class Pokemon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _backgroundColor;

        public string Name { get; set; }
        public string Sprite { get; set; }
        public List<string> Types { get; set; } = new List<string>();

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundColor)));
                }
            }
        }
    }
}
