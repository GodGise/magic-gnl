# Genera i pezzi del logo di GNL Studio: livelli bianchi trasparenti 1024x1024 (colorati poi dal gioco).
import math, sys
from PIL import Image, ImageDraw, ImageFilter
S=4; N=1024*S; C=N/2
def P(x,y): return (x*S,y*S)
def new(): return Image.new('L',(N,N),0)
def finish(im, name):
    im=im.resize((1024,1024),Image.LANCZOS)
    rgba=Image.new('RGBA',(1024,1024),(255,255,255,0)); rgba.putalpha(im)
    rgba.save(name)
    return im
def arc_ring(d,r,w,a0,a1):
    bb=[C-r*S,C-r*S,C+r*S,C+r*S]
    d.arc(bb,a0,a1,fill=255,width=int(w*S))
def pol(cx,cy,r,deg): 
    a=math.radians(deg); return (cx+r*math.cos(a), cy+r*math.sin(a))
def rune_tick(d,deg,r0,r1,w):
    p0=pol(512,512,r0,deg); p1=pol(512,512,r1,deg)
    d.line([P(*p0),P(*p1)],fill=255,width=int(w*S))
def quarter(k):
    im=new(); d=ImageDraw.Draw(im)
    a0=k*90-90+5; a1=k*90-90+85   # PIL: 0 = destra, senso orario
    arc_ring(d,478,7,a0,a1)
    arc_ring(d,452,2.5,a0+2,a1-2)
    arc_ring(d,398,2.5,a0+2,a1-2)
    for i in range(0,7):
        deg=k*90-90+5+i*(80/6)
        major = i in (0,3,6)
        rune_tick(d,deg,404 if not major else 392,446,3 if not major else 6)
    # piccoli rombi sulle tacche principali al centro del quarto
    mid=k*90-90+45
    cx,cy=pol(512,512,425,mid)
    s=15
    rad=[(-s*1.5,0),(0,-s*0.7),(s*1.5,0),(0,s*0.7)]
    a=math.radians(mid)
    pts=[P(cx+x*math.cos(a)-y*math.sin(a), cy+x*math.sin(a)+y*math.cos(a)) for x,y in rad]
    d.polygon(pts,fill=255)
    return im
def rot(pt,deg,c=(512,512)):
    a=math.radians(deg); x,y=pt[0]-c[0],pt[1]-c[1]
    return (c[0]+x*math.cos(a)-y*math.sin(a), c[1]+x*math.sin(a)+y*math.cos(a))
def diamond():
    im=new(); d=ImageDraw.Draw(im)
    def rh(r,w):
        pts=[P(512,512-r),P(512+r,512),P(512,512+r),P(512-r,512)]
        d.line(pts+[pts[0]],fill=255,width=int(w*S),joint='curve')
    rh(304,8); rh(278,2.4)
    # angoli: piccoli rombi pieni
    for (x,y) in [(512,208),(816,512),(512,816),(208,512)]:
        s=16
        d.polygon([P(x,y-s*1.3),P(x+s,y),P(x,y+s*1.3),P(x-s,y)],fill=255)
    # cerchio-lago al centro e anello interno
    r=150; d.ellipse([C-r*S,C-r*S,C+r*S,C+r*S],outline=255,width=int(3.5*S))
    r=132; d.ellipse([C-r*S,C-r*S,C+r*S,C+r*S],outline=255,width=int(1.8*S))
    # otto raggi corti dentro il cerchio (il sole sul lago)
    for k in range(16):
        deg=k*22.5
        rune_tick(d,deg,158,184 if k%2==0 else 172,2.4)
    return im
def blade():
    im=new(); d=ImageDraw.Draw(im)
    cx=512
    # lama sottile con punta lunga
    d.polygon([P(cx,128),P(cx+15,230),P(cx+13,590),P(cx-13,590),P(cx-15,230)],fill=255)
    # linea centrale scavata
    d.polygon([P(cx,210),P(cx+3.2,236),P(cx+3.2,572),P(cx-3.2,572),P(cx-3.2,236)],fill=0)
    # guardia curva con punte a rombo
    d.polygon([P(cx-112,604),P(cx+112,604),P(cx+84,632),P(cx-84,632)],fill=255)
    for sx in (-1,1):
        x=cx+sx*112
        d.polygon([P(x+sx*10,586),P(x+sx*30,612),P(x+sx*10,640),P(x-sx*2,612)],fill=255)
    d.polygon([P(cx-16,590),P(cx+16,590),P(cx+16,606),P(cx-16,606)],fill=255)
    # impugnatura con fasciatura e pomolo a rombo
    d.rectangle([P(cx-9,632)[0],P(cx-9,632)[1],P(cx+9,716)[0],P(cx+9,716)[1]],fill=255)
    for y in range(642,712,14):
        d.line([P(cx-9,y),P(cx+9,y+5)],fill=0,width=int(2.2*S))
    d.polygon([P(cx,712),P(cx+26,742),P(cx,776),P(cx-26,742)],fill=255)
    d.polygon([P(cx,728),P(cx+10,742),P(cx,760),P(cx-10,742)],fill=0)
    return im
if __name__=='__main__':
    out=sys.argv[1]
    for k in range(4): finish(quarter(k),f'{out}/anello-{k+1}.png')
    finish(diamond(),f'{out}/rombo.png')
    finish(blade(),f'{out}/spada.png')
