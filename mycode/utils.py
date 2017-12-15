import sys

# a simple implement for progessbar
class ProgressBar:
    def __init__(self, total_size):
        self.total_size = total_size
        self.f = sys.stdout
        self.status = 'downloading'

    def progress_show(self, count):
        count = min(count,self.total_size)
        percent = int(count/self.total_size*50)
        if self.status == 'finished':
            return
        self.f.write("downloading progress: [%s%s] %i%%\r"%('#'*percent,'-'*(50-percent),int(count/self.total_size*100)))
        self.f.flush()
        if percent == 50:
            self.f.write("\n")
            self.f.flush()
            self.status = 'finished'