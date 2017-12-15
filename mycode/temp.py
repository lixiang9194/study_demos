accuracy = []
with open('temp.txt') as f:
    line = f.readline()
    while line != '':
        if line.startswith('Epoch'):
            a = line.split(': ')[1].strip()
            accuracy.append(float(a))
        else:
            pass
        line = f.readline()
            

import matplotlib.pyplot as plt
plt.plot(accuracy)
plt.show()
