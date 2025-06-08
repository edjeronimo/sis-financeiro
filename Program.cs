using System;
using System.Linq;
using System.Collections.Generic;

namespace SistemaFinanceiro
{
    public static class ConsoleHelper
    {
        public static string GetString(string prompt, bool canBeEmpty = false)
        {
            string? input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (!canBeEmpty && string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Entrada não pode ser vazia. Tente novamente.");
                }
            } while (!canBeEmpty && string.IsNullOrWhiteSpace(input));
            return input?.Trim() ?? "";
        }

        public static decimal GetDecimal(string prompt)
        {
            decimal value;
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out value) && value >= 0)
                {
                    return value;
                }
                Console.WriteLine("Valor inválido. Por favor, insira um número não negativo.");
            }
        }

        public static Moeda GetMoeda(string prompt)
        {
            Moeda moeda;
            while (true)
            {
                Console.Write(prompt + " (BRL, USD, EUR): ");
                string? input = Console.ReadLine()?.ToUpper();
                if (Enum.TryParse(input, out moeda) && Enum.IsDefined(typeof(Moeda), moeda))
                {
                    return moeda;
                }
                Console.WriteLine("Moeda inválida. Tente novamente (BRL, USD, EUR).");
            }
        }

        public static DateTime GetDateTime(string prompt)
        {
            DateTime date;
            while (true)
            {
                Console.Write(prompt + " (dd/MM/yyyy HH:mm:ss): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out date))
                {
                    return date;
                }
                Console.WriteLine("Formato de data/hora inválido. Use dd/MM/yyyy HH:mm:ss.");
            }
        }

        public