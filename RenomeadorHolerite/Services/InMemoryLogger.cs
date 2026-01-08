namespace RenomeadorHolerite.Services
{
    public static class InMemoryLogger
    {
        // Guarda as últimas 100 linhas para não lotar a memória
        private static List<string> _logs = new List<string>();
        private static readonly object _lock = new object();

        public static void Log(string message)
        {
            lock (_lock)
            {
                // Escreve no terminal do Docker (para o Portainer ver)
                Console.WriteLine(message);

                // Adiciona na lista da memória (para o Site ver)
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                _logs.Add($"[{timestamp}] {message}");

                // Mantém apenas as últimas 200 linhas
                if (_logs.Count > 200)
                {
                    _logs.RemoveAt(0);
                }
            }
        }

        public static List<string> GetLogs()
        {
            lock (_lock)
            {
                return new List<string>(_logs); // Retorna uma cópia
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _logs.Clear();
            }
        }
    }
}