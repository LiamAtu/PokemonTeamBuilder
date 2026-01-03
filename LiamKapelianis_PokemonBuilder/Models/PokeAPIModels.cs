using System.Collections.Generic;

namespace LiamKapelianis_PokemonBuilder.Models
{
    public class PokemonResponse
    {
        public string name { get; set; }
        public Sprites sprites { get; set; }
        public List<TypeSlot> types { get; set; }
    }

    public class Sprites
    {
        public string front_default { get; set; }
    }

    public class TypeSlot
    {
        public TypeInfo type { get; set; }
    }

    public class TypeInfo
    {
        public string name { get; set; }
    }
}
