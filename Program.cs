using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Guerra atk = new Guerra(1000);
        Guerra def = new Guerra(585);
        int qtdSimu = 10_000;
        Stopwatch sw = new();

        object lockObject = new object();

        sw.Start();

        Parallel.For(0, qtdSimu, k =>
        {
            ThreadLocal<Random> localRand = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

            Guerra atkClone = new Guerra(1000);
            Guerra defClone = new Guerra(585);

            while (atkClone.soldados > 1 && defClone.soldados > 0)
            {
                atkClone.rodarDados(localRand.Value);
                defClone.rodarDados(localRand.Value);
                atkClone.batalha(defClone);
            }

            int atkVitorias = atkClone.soldados > defClone.soldados ? 1 : 0;
            int defVitorias = defClone.soldados > atkClone.soldados ? 1 : 0;

            lock (lockObject)
            {
                atk.vitorias += atkVitorias;
                def.vitorias += defVitorias;
            }
        });

        sw.Stop();
        Console.Write($"Atacantes: {(float)atk.vitorias / qtdSimu * 100}% de vitorias\nDefensores: {(float)def.vitorias / qtdSimu * 100}% de vitorias\n");
        Console.Write(sw.ElapsedMilliseconds + " ms");
    }
}

public class Guerra
{
    public int soldados { get; set; }
    public int vitorias { get; set; } = 0;
    public List<int> dados { get; private set; } 

    public Guerra(int soldiers)
    {
        this.soldados = soldiers;
        this.dados = new List<int>();
    }

    public void rodarDados(Random randNum)
    {
        dados.Clear();
        int maxRolls = soldados > 3 ? 3 : soldados;

        for (int i = 0; i < maxRolls; i++)
        {
            dados.Add((randNum.Next(6) + 1));
        }
        dados = dados.OrderByDescending(s => s).ToList();
    }

    public void batalha(Guerra def)
    {
        int rounds = Math.Min(3, Math.Min(this.soldados, def.soldados));
        
        for (int i = 0; i < rounds; i++)
        {
            if (this.dados[i] > def.dados[i])
            {
                def.soldados--;
            }
            else
            {
                this.soldados--;
            }
        }
    }
}
