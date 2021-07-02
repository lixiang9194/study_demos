import sys
import os
from PIL import Image, ImageOps
import scipy.misc
import numpy as np


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

# download vgg.mat from the url 
def download_vgg():
    VGG_DOWNLOAD_LINK = 'http://www.vlfeat.org/matconvnet/models/imagenet-vgg-verydeep-19.mat'
    VGG_MODEL = 'imagenet-vgg-verydeep-19.mat'
    DATA_DIR = './'
    EXPECTED_BYTES = 534904783
    vgg_path = download_file(VGG_DOWNLOAD_LINK, VGG_MODEL, DATA_DIR, EXPECTED_BYTES)
    return vgg_path

# resize the imgs to proper size tensor. eg:[128,224,224,3]
def fit_img(imgs_path, height=224, width=224):
    imgs = []
    for path in imgs_path:
        img = Image.open(path)
        img = ImageOps.fit(img, (width, height))
        imgs.append(np.array(img))
    return np.array(imgs)


def save_image(path, image):
    image = image[0]
    image = np.clip(image, 0, 255).astype('uint8')
    scipy.misc.imsave(path, image)


def generate_noise_image(content_image, height, width, noise_ratio=0.6):
    noise_image = np.random.uniform(-20, 20,
                                    (1, height, width, 3)).astype(np.float32)
    return noise_image * noise_ratio + content_image * (1 - noise_ratio)

if __name__ == '__main__':
    pass


