using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SeaOfSteel.Models;

public class Bateau
{
    public string Nom { get; set; }
    public int Taille { get; set; }
    public List<(int Row, int Col)> Positions { get; set; } = new();

    public bool EstCoule => Positions.Count == 0;
}
