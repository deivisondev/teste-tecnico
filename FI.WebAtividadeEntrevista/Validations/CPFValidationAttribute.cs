using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FI.WebAtividadeEntrevista.Validations
{
    public class CPFValidationAttribute : ValidationAttribute
    {
        public CPFValidationAttribute() : base("O CPF informado é inválido.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            var cpf = value.ToString().Replace(".", "").Replace("-", "");

            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
                return false;

            int[] peso1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma1 = 0;
            for (int i = 0; i < 9; i++)
            {
                soma1 += int.Parse(cpf[i].ToString()) * peso1[i];
            }
            int resto1 = soma1 % 11;
            int digito1 = resto1 < 2 ? 0 : 11 - resto1;

            int[] peso2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma2 = 0;
            for (int i = 0; i < 9; i++)
            {
                soma2 += int.Parse(cpf[i].ToString()) * peso2[i];
            }
            soma2 += digito1 * peso2[9];
            int resto2 = soma2 % 11;
            int digito2 = resto2 < 2 ? 0 : 11 - resto2;

            return cpf[9].ToString() == digito1.ToString() && cpf[10].ToString() == digito2.ToString();
        }
    }
}
