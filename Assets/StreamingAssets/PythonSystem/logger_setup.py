import sys

class UnityLogger:
    def __init__(self):
        self.buffer = ''

    def write(self, message):
        self.buffer += message
        if '\n' in self.buffer:
            line, self.buffer = self.buffer.split('\n', 1)
            if line.strip():
                unity_log(line)

    def flush(self):
        if self.buffer.strip():
            unity_log(self.buffer)
        self.buffer = ''
        
sys.stdout = UnityLogger()
sys.stderr = sys.stdout