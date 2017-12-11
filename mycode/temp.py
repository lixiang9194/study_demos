accuracy = []
with open('temp.txt') as f:
    line = f.readline()
    while line != '':
        if line.startswith('--'):
            line = f.readline()
            a = line.split('accuracy ')[1].strip()
            accuracy.append(float(a))
        else:
            line = f.readline()

import matplotlib.pyplot as plt
plt.plot(accuracy)
plt.show()
