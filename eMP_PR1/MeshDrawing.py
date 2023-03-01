import matplotlib.pyplot as plt

xB, xI, xF, yB, yI, yF = [], [], [], [], [], []

with open("Nodes/boundaryNodes.txt") as file:
    for line in file:
        xC, yC = line.split()
        xB.append(float(xC))
        yB.append(float(yC))

with open("Nodes/InnerNodes.txt") as file:
    for line in file:
        xC, yC = line.split()
        xI.append(float(xC))
        yI.append(float(yC))

with open("Nodes/FakeNodes.txt") as file:
    for line in file:
        xC, yC = line.split()
        xF.append(float(xC))
        yF.append(float(yC))

plt.grid()

plt.plot(xB, yB, 's', color='red', markersize=7)
plt.plot(xI, yI, 's', color='green', markersize=6)
plt.plot(xF, yF, 'x', color='black', markersize=6)
plt.show()