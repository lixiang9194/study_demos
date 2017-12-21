import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf
from scipy import misc
import numpy as np
import matplotlib.pyplot as plt
import math
import kernels


IMAGE_PATH = './data/friday.jpg'


def read_image(file_path):
    image = misc.imread(file_path)
    gray_arr = [0.299,0.587,0.114]
    image_gray = np.dot(image[...,:],gray_arr)
    image_gray = np.expand_dims(image_gray,0)
    image_gray = np.expand_dims(image_gray,3)
    return image_gray.astype(np.float32)


def convolve(image, kernel):
    image = tf.nn.conv2d(image, kernel, strides=[1,3,3,1], padding='SAME')
    return image[0,:,:,0]


def plot_image(img_list):
    line = math.ceil(math.sqrt(len(img_list)))
    col = math.ceil(len(img_list)/line)
    posi = 1
    for img in img_list:
        plt.subplot(line,line,posi)
        if np.size(img.shape) > 2:
            plt.imshow(img)
        if np.size(img.shape) > 2:
            plt.imshow(img)
        if np.size(img.shape) > 2:
            plt.imshow(img)
        else:
            plt.imshow(img,cmap='gray')
        posi += 1
    plt.show()


def main():
    sess = tf.InteractiveSession()
    origin_image = misc.imread(IMAGE_PATH)
    image = read_image(IMAGE_PATH)
    kernel_list = [kernels.BLUR_FILTER, kernels.SHARPEN_FILTER, kernels.EDGE_FILTER, 
                    kernels.TOP_SOBEL, kernels.EMBOSS_FILTER]
    image_conv = [origin_image,image[0,:,:,0]]
    for kernel in kernel_list:
        img = convolve(image,kernel)
        image_conv.append(img.eval())
    plot_image(image_conv)


if __name__ == '__main__':
    main()