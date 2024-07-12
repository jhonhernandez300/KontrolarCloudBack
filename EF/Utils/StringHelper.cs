using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF.Utils
{
    public class StringHelper
    {
        public static string EliminateFirstAndLast(string documentNumber)
        {
            // Eliminar el primer carácter
            if (documentNumber.Length > 1)
            {
                documentNumber = documentNumber.Substring(1);
            }
            else
            {
                documentNumber = string.Empty; // La cadena queda vacía si tiene menos de 2 caracteres
            }

            // Eliminar el último carácter
            if (documentNumber.Length > 1)
            {
                documentNumber = documentNumber.Substring(0, documentNumber.Length - 1);
            }
            else
            {
                documentNumber = string.Empty; // La cadena queda vacía si tiene menos de 2 caracteres
            }

            return documentNumber;
        }

    }
}
