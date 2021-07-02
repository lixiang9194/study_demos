import sys
import os

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

def download_file(download_url, file_name, data_dir, expected_bytes):
    file_path = data_dir + file_name
    if os.path.exists(file_path):
        print("Dataset ready")
        return file_path
    with closing(requests.get(download_url, stream=True)) as r:
        total_size = int(r.headers['content-length'])
        download_size = 0
        p = ProgressBar(total_size)
        with open(file_path,'wb') as f:
            for chunk in r.iter_content(CHUNK_SIZE):
                download_size += CHUNK_SIZE
                p.progress_show(download_size)
                f.write(chunk)
    file_stat = os.stat(file_path)
    if file_stat.st_size == expected_bytes:
        print('Successfully download the file', file_name)
    else:
        raise Exception('File '+ file_name +
            ' might be corrupted. You should try downloading manually.')
    return file_path