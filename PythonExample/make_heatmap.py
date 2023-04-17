import numpy as np
from matplotlib import pyplot as plt
from pdb import set_trace as bp


def main():
    coordinates = np.genfromtxt("MouseCoords.txt",delimiter=",")
    heatmap = make_heatmap(coordinates)
    heatmap = heatmap*255/np.max(heatmap)
    plt.imshow(heatmap)
    plt.savefig("Heatmap.pdf",bbox_inches='tight',dpi=100)

def make_heatmap(coordinates):
    screen_size = (1080,2048)
    heatmap = np.zeros(screen_size)
    for coordinate in coordinates:

        gx = gkern(coordinate[0],screen_size[0])
        gy = gkern(coordinate[1],screen_size[1])
        kernel = np.multiply.outer(gx,gy)
        heatmap += kernel
        
    return heatmap


def gkern(c,l, sig=10.):
    """\
    creates gaussian kernel with side length `l` and a sigma of `sig`, centered in `c`.
    """
    ax = np.arange(-c+1,l-c+1)
    kernel = np.exp(-0.5 * np.square(ax) / np.square(sig))
    return kernel

if __name__ == "__main__":
    main()