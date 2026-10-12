# Genera i pezzi del logo di GNL Studio: livelli bianchi trasparenti (colorati poi dal gioco).
# Uso: python3 genera_logo.py cartella_di_uscita
# Pezzi 1024x1024: anello-1..4 (archi), anello-esterno, rombo, spada, onda, raggi. Rune 256x256: runa-1..7.
import math, sys
from PIL import Image, ImageDraw
S=4
def new(n): return Image.new('L',(n*S,n*S),0)
def finish(im, name, n):
    im=im.resize((n,n),Image.LANCZOS)
    rgba=Image.new('RGBA',(n,n),(255,255,255,0)); rgba.putalpha(im)
    rgba.save(name)
def P(x,y): return (x*S,y*S)
def pol(cx,cy,r,deg):
    a=math.radians(deg); return (cx+r*math.cos(a), cy+r*math.sin(a))
def circle(d,r,w,c=512):
    d.ellipse([(c-r)*S,(c-r)*S,(c+r)*S,(c+r)*S],outline=255,width=int(w*S))
def arc(d,r,w,a0,a1):
    d.arc([(512-r)*S,(512-r)*S,(512+r)*S,(512+r)*S],a0,a1,fill=255,width=int(w*S))
def tick(d,deg,r0,r1,w):
    d.line([P(*pol(512,512,r0,deg)),P(*pol(512,512,r1,deg))],fill=255,width=int(w*S))
def rhomb(d,x,y,rx,ry,deg=0,fill=255):
    a=math.radians(deg)
    pts=[(-rx,0),(0,-ry),(rx,0),(0,ry)]
    d.polygon([P(x+px*math.cos(a)-py*math.sin(a), y+px*math.sin(a)+py*math.cos(a)) for px,py in pts],fill=fill)

# angoli (gradi PIL, 0 = destra, senso orario) delle 7 rune di ogni quarto
def angoli(k): return [k*90-90+8+i*(74/6) for i in range(7)]

def quarter(k):
    im=new(1024); d=ImageDraw.Draw(im)
    a0=k*90-90+4; a1=k*90-90+86
    arc(d,478,7,a0,a1)          # anello esterno spesso
    arc(d,490,1.6,a0+3,a1-3)    # filo sottile fuori
    arc(d,452,2.4,a0+1,a1-1)    # bordo esterno della fascia delle rune
    arc(d,396,2.4,a0+1,a1-1)    # bordo interno
    ang=angoli(k)
    # separatori sottili tra una runa e l'altra, e agli estremi
    seps=[ang[0]-(ang[1]-ang[0])/2]+[(ang[i]+ang[i+1])/2 for i in range(6)]+[ang[6]+(ang[1]-ang[0])/2]
    for deg in seps: tick(d,deg,398,450,2.2)
    # tacche fuori dall'anello
    for i in range(0,17):
        deg=a0+2+i*(a1-a0-4)/16
        tick(d,deg,484,484+(15 if i%4==0 else 8),3 if i%4==0 else 1.8)
    # rombo grande a metà del quarto, sull'anello esterno
    mx,my=pol(512,512,478,k*90-90+45)
    rhomb(d,mx,my,26,11,k*90-90+45+90)
    return im

def esterno():
    im=new(1024); d=ImageDraw.Draw(im)
    circle(d,507,1.6)
    for i in range(144):
        deg=i*2.5
        major=i%6==0
        tick(d,deg,497 if not major else 488,505,1.6 if not major else 3)
    for deg in (0,90,180,270): rhomb(d,*pol(512,512,500,deg),9,4,deg+90)
    return im

def rombo():
    im=new(1024); d=ImageDraw.Draw(im)
    def rh(r,w):
        pts=[P(512,512-r),P(512+r,512),P(512,512+r),P(512-r,512)]
        d.line(pts+[pts[0]],fill=255,width=int(w*S),joint='curve')
    rh(304,8); rh(286,1.8); rh(262,2.4)
    for (x,y,deg) in [(816,512,0),(512,816,90),(208,512,0)]:
        d.polygon([P(x,y-26),P(x+17,y),P(x,y+26),P(x-17,y)],fill=255)
    # il rombo in alto lo copre la spada: restano due tacche ai lati
    for sx in (-1,1): d.polygon([P(512+sx*40,236),P(512+sx*52,226),P(512+sx*64,236),P(512+sx*52,246)],fill=255)
    circle(d,150,3.5); circle(d,132,1.8); circle(d,100,1.2)
    for k in range(32):
        tick(d,k*11.25,158,190 if k%2==0 else 174,2.4 if k%2==0 else 1.6)
    return im

def spada():
    # spada con la punta in giù, piantata nel lago
    im=new(1024); d=ImageDraw.Draw(im); cx=512
    d.polygon([P(cx-17,264),P(cx+17,264),P(cx+15,482),P(cx,566),P(cx-15,482)],fill=255)
    d.polygon([P(cx-3.4,282),P(cx+3.4,282),P(cx+2.4,470),P(cx,498),P(cx-2.4,470)],fill=0)
    for sx in (-1,1):
        pts=[(14,232),(70,224),(128,238),(160,284),(118,266),(66,267),(14,266)]
        d.polygon([P(cx+sx*x,y) for x,y in pts],fill=255)
        d.polygon([P(cx+sx*x,y) for x,y in [(40,240),(86,236),(124,248),(90,252),(46,252)]],fill=0)
    rhomb(d,cx,249,26,24,0)
    rhomb(d,cx,249,9,9,0,fill=0)
    d.rectangle([P(cx-8.5,150)[0],P(0,150)[1],P(cx+8.5,0)[0],P(0,232)[1]],fill=255)
    for y in range(158,230,12): d.line([P(cx-8.5,y),P(cx+8.5,y+5)],fill=0,width=int(2.2*S))
    d.polygon([P(cx,82),P(cx+25,120),P(cx,162),P(cx-25,120)],fill=255)
    d.polygon([P(cx,100),P(cx+10,120),P(cx,144),P(cx-10,120)],fill=0)
    return im

def onda():
    im=new(1024); d=ImageDraw.Draw(im); circle(d,500,5); return im

def raggi():
    # raggi sottili che partono da fuori dell'anello e sfumano verso il bordo della tela
    im=Image.new('L',(1024,1024),0); n=48
    for i in range(n):
        deg=i*360/n; lungo=(i%3==0); rr=508 if lungo else 420
        r0=330; w=0.9 if lungo else 0.5
        layer=Image.new('L',(1024*2,1024*2),0); dl=ImageDraw.Draw(layer)
        a=math.radians(deg)
        p0=(1024+r0*2*math.cos(a),1024+r0*2*math.sin(a)); p1=(1024+rr*2*math.cos(a),1024+rr*2*math.sin(a))
        nx,ny=-math.sin(a),math.cos(a); wd=(8 if lungo else 5)
        dl.polygon([(p0[0]+nx*wd,p0[1]+ny*wd),(p0[0]-nx*wd,p0[1]-ny*wd),p1],fill=255)
        layer=layer.resize((1024,1024),Image.LANCZOS)
        # sfumatura: piena a r0, zero a rr
        px=layer.load(); gl=Image.new('L',(1024,1024),0); g=gl.load()
        for y in range(0,1024):
            for x in range(0,1024):
                v=px[x,y]
                if v:
                    r=math.hypot(x-512,y-512); f=max(0.0,min(1.0,(rr-r)/(rr-r0)))
                    g[x,y]=int(v*f*f)
        im=Image.fromarray(__import__('numpy').maximum(__import__('numpy').array(im),__import__('numpy').array(gl)))
    return im

GLIFI=[
 [[(0,-1),(0,1)],[(0,-.5),(.5,-.9)],[(0,0),(.5,-.4)]],
 [[(-.4,-1),(-.4,1)],[(-.4,-1),(.4,-.4),(.4,1)]],
 [[(0,-1),(0,1)],[(0,-.6),(.45,-.2),(0,.2)]],
 [[(0,-1),(0,1)],[(0,-1),(.5,-.5)],[(0,-.3),(.5,.2)]],
 [[(0,-1),(0,1)],[(0,-1),(.45,-.6),(0,-.2),(.45,.6)]],
 [[(.4,-.8),(-.2,0),(.4,.8)],[(-.2,-1),(-.2,1)]],
 [[(0,-1),(.45,-.5),(0,0),(-.45,-.5),(0,-1)],[(0,0),(-.45,.9)],[(0,0),(.45,.9)]],
]
def runa(i):
    n=256; im=new(n); d=ImageDraw.Draw(im); c=n/2
    sx,sy=62,100; w=14
    for linea in GLIFI[i]:
        pts=[P(c+x*sx,c+y*sy) for x,y in linea]
        d.line(pts,fill=255,width=w*S,joint='curve')
        for p in (pts[0],pts[-1]):
            d.ellipse([p[0]-w*S/2,p[1]-w*S/2,p[0]+w*S/2,p[1]+w*S/2],fill=255)
    return im

if __name__=='__main__':
    out=sys.argv[1]
    for k in range(4): finish(quarter(k),f'{out}/anello-{k+1}.png',1024)
    finish(esterno(),f'{out}/anello-esterno.png',1024)
    finish(rombo(),f'{out}/rombo.png',1024)
    finish(spada(),f'{out}/spada.png',1024)
    finish(onda(),f'{out}/onda.png',1024)
    raggi().save(f'{out}/_raggi_tmp.png')
    r=Image.open(f'{out}/_raggi_tmp.png').convert('L')
    rgba=Image.new('RGBA',(1024,1024),(255,255,255,0)); rgba.putalpha(r); rgba.save(f'{out}/raggi.png')
    import os; os.remove(f'{out}/_raggi_tmp.png')
    for i in range(7): finish(runa(i),f'{out}/runa-{i+1}.png',256)
