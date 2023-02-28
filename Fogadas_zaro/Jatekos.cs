using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fogadas_zaro
{
    class Jatekos
    {
        int nemzet_id;
        string jatekos_nev;
        string pozicio;

        public Jatekos(int nemzet_id, string jatekos_nev, string pozicio)
        {
            this.nemzet_id = nemzet_id;
            this.jatekos_nev = jatekos_nev;
            this.pozicio = pozicio;
        }

        public int Nemzet_id { get => nemzet_id; set => nemzet_id = value; }
        public string Jatekos_nev { get => jatekos_nev; set => jatekos_nev = value; }
        public string Pozicio { get => pozicio; set => pozicio = value; }


    }
}
