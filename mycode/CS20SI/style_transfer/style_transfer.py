import tensorflow as tf
from utils import save_image, fit_img, generate_noise_image
from vgg import VGG_NET
import numpy as np

STYLE_IMAGE = './data/starry_night.jpg'
CONTENT_IMAGE = './data/girl.JPG'
IMAGE_HEIGHT = 250
IMAGE_WIDTH = 333
NOISE_RATIO = 0.6

CONTENT_WEIGHT = 0.01
STYLE_WEIGHT = 1

STYLE_LAYERS = ['conv1_1', 'conv2_1', 'conv3_1', 'conv4_1', 'conv5_1']
W = [0.5, 1.0, 1.5, 3.0, 4.0]

CONTENT_LAYER = 'conv4_2'
ITERS = 300
LR = 2.0

MEAN_PIXELS = np.array([123.68, 116.79, 103.939]).reshape([1,1,1,3])

def create_content_loss(p, f):
    """ Calculate the loss between the feature representation
    of the content image and the generated image
    """
    return tf.reduce_sum((f - p) ** 2) / (4.0 * p.size)


def gram_matrix(F, N, M):
    """
    Create and return the gram matrix for tensor F
    """
    F = tf.reshape(F, (M, N))
    return tf.matmul(tf.transpose(F), F)


def single_style_loss(a, g):
    """
    Calculate the style loss at a certain layer
    """
    N = a.shape[3] # number of filters
    M = a.shape[1] * a.shape[2] # height times width of the feature map
    A = gram_matrix(a, N, M)
    G = gram_matrix(g, N, M)
    return tf.reduce_sum((G - A) ** 2 / ((2 * N * M) ** 2))


def create_style_loss(A, vgg_model):
    """return the total style loss
    """
    n_layers = len(STYLE_LAYERS)
    E = [single_style_loss((A[i]), vgg_model.graph[STYLE_LAYERS[i]]) for  i in range(n_layers)]
    return sum([W[i] * E[i] for i in range(n_layers)])


def create_losses(vgg_model, content_image, style_image):
    with tf.variable_scope('loss') as scope:
        with tf.Session() as sess:
            sess.run(vgg_model.input_image.assign(content_image))
            p = sess.run(vgg_model.graph[CONTENT_LAYER])
        content_loss = create_content_loss(p, vgg_model.graph[CONTENT_LAYER])

        with tf.Session() as sess:
            sess.run(vgg_model.input_image.assign(style_image))
            A = sess.run([vgg_model.graph[layer_name] for layer_name in STYLE_LAYERS])
        style_loss = create_style_loss(A, vgg_model)

        total_loss = CONTENT_WEIGHT * content_loss + STYLE_WEIGHT * style_loss

    return content_loss, style_loss, total_loss


def create_summary(model):
    with tf.name_scope('summary'):
        tf.summary.scalar('content loss', model['content_loss'])
        tf.summary.scalar('style loss', model['style_loss'])
        tf.summary.scalar('total loss', model['total_loss'])
        tf.summary.histogram('content loss', model['content_loss'])
        tf.summary.histogram('style loss', model['style_loss'])
        tf.summary.histogram('total loss', model['total_loss'])
        return tf.summary.merge_all()


def train(model, vgg_model, initial_image):
    with tf.Session() as sess:
        saver = tf.train.Saver()
        writer = tf.summary.FileWriter('./data/style_trans', sess.graph)
        sess.run(tf.global_variables_initializer())
        ckpt = tf.train.get_checkpoint_state('./data/style_trans')
        if ckpt and ckpt.model_checkpoint_path:
            saver.restore(sess, ckpt.model_checkpoint_path)
        initial_step = model['global_step'].eval()
        sess.run(vgg_model.input_image.assign(initial_image))
        for index in range(initial_step, ITERS):
            sess.run(model['optimizer'])
            if(index + 1) % 20 == 0:
                gen_image, total_loss, summary = sess.run([vgg_model.input_image, model['total_loss'], model['summary_op']])
                gen_image = gen_image + MEAN_PIXELS
                writer.add_summary(summary, global_step=index)
                print('step {}\n    sum: {:5.1f}'.format(index + 1, np.sum(gen_image)))
                print('     loss: {:5.1f}'.format(total_loss))

                filename = './data/%d.png' % (index)
                save_image(filename, gen_image)
                saver.save(sess, './data/style_trans/style_trans', index)

def main():
    vgg_path = './data/imagenet-vgg-verydeep-19.mat'
    synset_path = './data/synset.ini'
    vgg_model = VGG_NET(vgg_path, synset_path)
    vgg_model.construct_vgg_net(batch_size=1, image_height=IMAGE_HEIGHT, image_width=IMAGE_WIDTH)
    model = {}
    model['global_step'] = tf.Variable(0, dtype=tf.int32, trainable=False, name='global_step')
    content_image = fit_img([CONTENT_IMAGE], IMAGE_HEIGHT, IMAGE_WIDTH)
    content_image = content_image - MEAN_PIXELS
    style_image = fit_img([STYLE_IMAGE], IMAGE_HEIGHT, IMAGE_WIDTH)
    style_image = style_image - MEAN_PIXELS
    model['content_loss'], model['style_loss'], model['total_loss'] = create_losses(vgg_model, content_image, style_image)
    model['optimizer'] = tf.train.AdamOptimizer(LR).minimize(model['total_loss'], global_step=model['global_step'])
    model['summary_op'] = create_summary(model)
    initial_image = generate_noise_image(content_image, IMAGE_HEIGHT, IMAGE_WIDTH, NOISE_RATIO)
    save_image('./data/initial.png', initial_image)
    train(model, vgg_model, initial_image)

if __name__ == '__main__':
    main()




