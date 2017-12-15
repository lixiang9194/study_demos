import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf
from process_data import process_data
from tensorflow.contrib.tensorboard.plugins import projector

# define the constants
VOCAB_SIZE = 50000
EMBED_SIZE = 128
NUM_SAMPLED = 64
BATCH_SIZE = 128
LEARNING_RATE = 1.0
NUM_TRAIN_STEPS = 100000
SKIP_WINDOW = 1

batch_gen = process_data(VOCAB_SIZE, BATCH_SIZE, SKIP_WINDOW)

# denfine the placeholders
with tf.name_scope('data'):
    center_words = tf.placeholder(tf.int32, shape=[BATCH_SIZE], name='center_words')
    target_words = tf.placeholder(tf.int32, shape=[BATCH_SIZE,1], name='target_words')
    global_step = tf.Variable(0, dtype=tf.int32, trainable=False, name='global_step')
# define the weight
with tf.name_scope('embed'):
    embed_matrix = tf.Variable(tf.random_uniform([VOCAB_SIZE,EMBED_SIZE], -1.0, 1.0), name='embed_matrix')

# inference
with tf.name_scope('loss'):
    embed = tf.nn.embedding_lookup(embed_matrix, center_words, name='embed')
# denfine the loss function
    nce_weight = tf.Variable(tf.truncated_normal([VOCAB_SIZE, EMBED_SIZE], stddev=1.0/EMBED_SIZE ** 0.5), name='nce_weight')
    nce_bias = tf.Variable(tf.zeros([VOCAB_SIZE]), name='nce_bias')
    loss = tf.reduce_mean(tf.nn.nce_loss(weights=nce_weight,
                                    biases=nce_bias,
                                    labels=target_words,
                                    inputs=embed,
                                    num_sampled=NUM_SAMPLED,
                                    num_classes=VOCAB_SIZE), name='loss')
# define optimizer
optimizer = tf.train.GradientDescentOptimizer(LEARNING_RATE).minimize(loss, global_step=global_step)

# define summary
with tf.name_scope('summary'):
    tf.summary.scalar('loss', loss)
    tf.summary.histogram('histogram loss', loss)
    summary_op = tf.summary.merge_all()

# execute the computation

saver = tf.train.Saver()
with tf.Session() as sess:
    sess.run(tf.global_variables_initializer())
    writer = tf.summary.FileWriter('./data/graphs/word2vec', sess.graph)
    # if checkpoint exists, restore from checkpoint
    ckpt = tf.train.get_checkpoint_state(os.path.dirname('./data/checkpoints/checkpoint'))
    if ckpt and ckpt.model_checkpoint_path:
        saver.restore(sess, ckpt.model_checkpoint_path)
    
    average_loss = 0.0
    inital_step = global_step.eval()
    for index in xrange(inital_step, inital_step + NUM_TRAIN_STEPS):
        batch = batch_gen.next()
        loss_batch, _, summary = sess.run([loss, optimizer, summary_op],
                feed_dict={center_words:batch[0], target_words:batch[1]})
        writer.add_summary(summary, global_step=index)
        average_loss += loss_batch
        if(index+1)%2000 == 0:
            print('average loss at step {0}: {1}'.format(index+1, average_loss/(index+1)))
            saver.save(sess, './data/checkpoints/skip-gram', index)
            average_loss = 0
    writer.close()

    # visualize the embeddings
    final_embed_matrxi = sess.run(embed_matrix)
    embedding_var = tf.Variable(final_embed_matrxi[:1000], name='embedding')
    sess.run(embedding_var.initializer)

    config = projector.ProjectorConfig()
    summary_writer = tf.summary.FileWriter('./data/processed')

    # add embedding to the config file
    embedding = config.embeddings.add()
    embedding.tensor_name = embedding_var.name

    # link this tensor to it's metadata file
    embedding.metadata_path = 'vocab_1000.tsv'

    # save a configuration file that tensorboard will read during startup
    projector.visualize_embeddings(summary_writer, config)
    saver_embed = tf.train.Saver([embedding_var])
    saver_embed.save(sess,'./data/processed/word2vec.ckpt',1)















