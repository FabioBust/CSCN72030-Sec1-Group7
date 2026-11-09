using System.IO;
using System;

namespace AquaGuard.Data
{
    public class FileDataProvider
    {
        private readonly string _filePath;
        private int _currentIndex;

        public FileDataProvider(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "File path cannot be empty.",
                    nameof(filePath));
            }

            _filePath = filePath;
            _currentIndex = 0;
        }
        public string? ReadNextRecord()
        {
            try
            {
                string[] records = File.ReadAllLines(_filePath);

                if (records.Length == 0)
                    return null;

                _currentIndex %= records.Length;

                string record = records[_currentIndex];

                _currentIndex = (_currentIndex + 1) % records.Length;

                return string.IsNullOrWhiteSpace(record)
                    ? null
                    : record;
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }
        public void Reset()
        {
            _currentIndex = 0;
        }
    }
}