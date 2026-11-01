
//  MODELO: Licencia. Guarda el resultado de leer licencia


// Módulos que la licencia puede habilitar o bloquear
public static class Modulos
{
    public const string Socios = "socios";
    public const string Membresias = "membresias";
    public const string Pagos = "pagos";
    public const string Asistencias = "asistencias";
}

public class Licencia
{
    public string Plan { get; }         // "COMPLETA", "BASICA" o "NINGUNA"
    public bool EsValida { get; }
    public string Motivo { get; }       // explica por qué es válida o inválida
    public string Vencimiento { get; }  // texto para mostrar

    public Licencia(string plan, bool esValida, string motivo, string vencimiento)
    {
        this.Plan = plan;
        this.EsValida = esValida;
        this.Motivo = motivo;
        this.Vencimiento = vencimiento;
    }

    // Habilita o restringe módulos según el plan
    public bool PermiteModulo(string modulo)
    {
        if (!EsValida)
        {
            return false;
        }

        // El plan COMPLETA habilita todo
        if (Plan == "COMPLETA")
        {
            return true;
        }

        // El plan BASICA solo habilita Socios y Asistencias
        return modulo == Modulos.Socios || modulo == Modulos.Asistencias;
    }

    public override string ToString()
    {
        if (!EsValida)
        {
            return "Licencia inválida: " + Motivo;
        }
        return "Plan: " + Plan + " | Estado: válida | Vencimiento: " + Vencimiento;
    }
}
