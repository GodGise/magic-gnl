# Genera la mappa vista dall alto di Villaggio Lago Nero (Docs/mappe/villaggio-lago-nero.png).
# Uso: dalla cartella Docs/mappe, python3 genera-mappa-villaggio.py (servono Pillow e numpy).
import math, random
import numpy as np
from PIL import Image, ImageDraw, ImageFont

random.seed(5)
W_M, H_M = 210, 170
PX, SS = 14, 2
S = PX * SS
MX, MY, BOT = 60 * SS, 150 * SS, 170 * SS
IW, IH = int(W_M * S + 2 * MX), int(H_M * S + MY + BOT)
R = 2  # risoluzione maschera: celle da 0.5 m

img = Image.new("RGB", (W_M * S, H_M * S), "#d9dcc4")
d = ImageDraw.Draw(img)
occ = Image.new("L", (W_M * R, H_M * R), 0)   # occupazione (case, strade, piazza...)
od = ImageDraw.Draw(occ)

def P(x, y): return (x * S, y * S)
def M(x, y): return (x * R, y * R)

FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSerif.ttf"
FONTB = "/usr/share/fonts/truetype/dejavu/DejaVuSerif-Bold.ttf"
def f(sz, b=False): return ImageFont.truetype(FONTB if b else FONT, sz * SS)

# terreno
for _ in range(260):
    x, y, r = random.uniform(0, W_M), random.uniform(0, H_M), random.uniform(2, 7)
    d.ellipse([P(x - r, y - r * .7), P(x + r, y + r * .7)], fill=random.choice(["#d2d6bb", "#dfe0ca", "#cfd3b6"]))

# lago
def riva(y): return 166 + 7 * math.sin(y / 17) + 3 * math.sin(y / 6 + 1) - 6 * math.cos(y / 40)
shore = [(riva(y), y) for y in range(-2, H_M + 3)]
d.polygon([P(x - 3.5, y) for x, y in shore] + [P(W_M, H_M), P(W_M, 0)], fill="#c8bb9a")
d.polygon([P(*p) for p in shore] + [P(W_M, H_M), P(W_M, 0)], fill="#1f242b")
od.polygon([M(x - 3, y) for x, y in shore] + [M(W_M, H_M), M(W_M, 0)], fill=1)
for _ in range(80):
    y = random.uniform(2, H_M - 2); x0 = random.uniform(riva(y) + 6, W_M - 8); l = random.uniform(3, 9)
    d.line([P(x0, y), P(x0 + l, y)], fill="#323944", width=2 * SS)
d.line([P(*p) for p in shore], fill="#3b3a33", width=3 * SS)

# strade
STRADE = []
def strada(pts, w, col="#bca98a"):
    STRADE.append((pts, w))
    pp = [P(x, y) for x, y in pts]
    d.line(pp, fill=col, width=int(w * S), joint="curve")
    for p in pp:
        r = w * S / 2; d.ellipse([p[0] - r, p[1] - r, p[0] + r, p[1] + r], fill=col)
    od.line([M(x, y) for x, y in pts], fill=1, width=int(w * R) + 1)
    for x, y in pts:
        r = w / 2; od.ellipse([M(x - r, y - r), M(x + r, y + r)], fill=1)

cx, cy = 96, 86
principali = [
    [(-2, 112), (20, 108), (42, 100), (62, 93), (cx - 15, cy + 1)],            # ovest, dal bosco
    [(cx - 2, cy + 14), (90, 110), (84, 132), (80, 155), (78, H_M + 2)],       # sud, verso le grotte
    [(cx - 7, cy - 13), (88, 62), (78, 46), (68, 37)],                         # nord, tempio
    [(cx + 16, cy + 2), (122, 92), (142, 96), (riva(97) - 1, 97)],             # est, molo
]
vicoli = [
    [(62, 93), (64, 74), (78, 62), (88, 62)],          # anello ovest-nord
    [(88, 62), (104, 60), (122, 66), (130, 80), (122, 92)],   # anello nord-est
    [(122, 92), (126, 108), (112, 118), (90, 112)],    # anello sud-est
    [(90, 112), (72, 116), (58, 108), (62, 93)],       # anello sud-ovest
    [(126, 108), (136, 118)],                          # verso la casa dell'eroe
    [(42, 100), (44, 120), (58, 130), (84, 132)],      # contrada sud-ovest
    [(104, 60), (108, 44), (100, 34)],                 # verso nord-est
]
for s in principali: strada(s, 3.4)
for s in vicoli: strada(s, 2.2)

# piazzetta
pts = []
for i in range(40):
    a = i / 40 * 2 * math.pi
    r = 16 + 1.2 * math.sin(a * 3 + 1)
    pts.append((cx + r * math.cos(a), cy + r * 0.85 * math.sin(a)))
d.polygon([P(*p) for p in pts], fill="#c9bea6", outline="#8e826b", width=2 * SS)
od.polygon([M(*p) for p in pts], fill=1)
for _ in range(110):
    a, r = random.uniform(0, 6.28), random.uniform(0, 11)
    x, y = cx + r * math.cos(a), cy + r * .8 * math.sin(a)
    d.rectangle([P(x - .6, y - .4), P(x + .6, y + .4)], outline="#b3a78f", width=SS)
d.ellipse([P(cx - 1.8, cy - 1.8), P(cx + 1.8, cy + 1.8)], fill="#7d7a72", outline="#3d3a34", width=2 * SS)
d.ellipse([P(cx - 1, cy - 1), P(cx + 1, cy + 1)], fill="#1f242b")

# edifici
TETTI = ["#8b6a4b", "#7d5f43", "#94735a", "#6f5640", "#866650", "#7a6a5c", "#5f5a55"]
def angoli(x, y, w, h, ang):
    a = math.radians(ang); ca, sa = math.cos(a), math.sin(a)
    return [(x + dx * ca - dy * sa, y + dx * sa + dy * ca) for dx, dy in [(-w/2, -h/2), (w/2, -h/2), (w/2, h/2), (-w/2, h/2)]]
def libero_poly(c):
    t = Image.new("L", occ.size, 0); ImageDraw.Draw(t).polygon([M(*p) for p in c], fill=1)
    a = np.array(t, bool)
    if not a.any(): return False
    return not (a & np.array(occ, bool)).any()
DISEGNI = []
def edificio(x, y, w, h, ang, tetto=None, muro="#3d2f22", segna=True, porta=None):
    c = angoli(x, y, w, h, ang)
    if segna: od.polygon([M(*p) for p in c], fill=1)
    DISEGNI.append((c, tetto or random.choice(TETTI), muro, w, h, x, y, ang, porta))
def locale(x, y, ang, lx, ly):
    a = math.radians(ang); ca, sa = math.cos(a), math.sin(a)
    return (x + lx * ca - ly * sa, y + lx * sa + ly * ca)
def disegna_edifici():
    for c, tetto, muro, w, h, x, y, ang, porta in DISEGNI:
        d.polygon([(P(*p)[0] + .7 * S, P(*p)[1] + .7 * S) for p in c], fill="#9fa18c")
    for c, tetto, muro, w, h, x, y, ang, porta in DISEGNI:
        d.polygon([P(*p) for p in c], fill=tetto, outline=muro, width=2 * SS)
        if w >= h: e = [locale(x, y, ang, -w/2, 0), locale(x, y, ang, w/2, 0)]
        else: e = [locale(x, y, ang, 0, -h/2), locale(x, y, ang, 0, h/2)]
        d.line([P(*p) for p in e], fill=muro, width=2 * SS)
        if porta is not None:
            lx, ly = porta
            sg = 1 if ly > 0 else -1
            pts = [locale(x, y, ang, lx - .8, ly), locale(x, y, ang, lx + .8, ly),
                   locale(x, y, ang, lx + .8, ly - sg * .9), locale(x, y, ang, lx - .8, ly - sg * .9)]
            d.polygon([P(*p) for p in pts], fill="#e7c98a", outline="#2a1d12", width=SS)

etichette = []
def etichetta(x, y, t, sz=16, b=True, col="#2b241c"): etichette.append((x, y, t, sz, b, col))

# --- anello di edifici intorno alla piazzetta, porte verso il centro
def bordo_piazza(a):
    r = 16 + 1.2 * math.sin(a * 3 + 1)
    return cx + r * math.cos(a), cy + r * 0.85 * math.sin(a)
def posa_anello(adeg, w, h, tetto=None, nome=None, sz=16):
    a = math.radians(adeg)
    bx, by = bordo_piazza(a)
    ox, oy = bx - cx, by - cy; L = math.hypot(ox, oy); ox, oy = ox / L, oy / L
    x, y = bx + ox * (0.8 + h / 2), by + oy * (0.8 + h / 2)
    ang = math.degrees(math.atan2(oy, ox)) + 90
    if not libero_poly(angoli(x, y, w - 1.8, h - 1.8, ang)): return False
    edificio(x, y, w, h, ang, tetto, porta=(0, h / 2))
    if nome:
        lx, ly = x + ox * (h / 2 + 3), y + oy * (h / 2 + 3)
        etichetta(lx, ly, nome, sz)
    return True
VARCHI = [9, 100, 175, 241]          # dove le strade escono dalla piazzetta (gradi)
def in_varco(adeg):
    return any(abs((adeg - v + 180) % 360 - 180) < 9 for v in VARCHI)
posa_anello(278, 9, 7, "#6f4f36", "Mercante")
posa_anello(322, 11, 9, "#5e4029", "Taverna", 18)
posa_anello(208, 8, 7, "#6f4f36", "Fabbro")
posa_anello(138, 8, 7, "#6f4f36", "Erborista")
n_spec = len(DISEGNI)
adeg = 0.0
while adeg < 360:
    w, h = random.uniform(4.2, 5.5), random.uniform(7, 8.5)
    if in_varco(adeg): adeg += 2; continue
    bx, by = bordo_piazza(math.radians(adeg))
    rin = math.hypot(bx - cx, by - cy) + 0.8
    if posa_anello(adeg, w, h): adeg += math.degrees((w + 0.05) / rin)
    elif posa_anello(adeg, 3.6, h): adeg += math.degrees(3.65 / rin)
    else: adeg += 1

# altri edifici speciali
edificio(60, 26, 16, 10, -20, "#6d6a63", "#2c2a26", porta=(8, 0) if False else None)
d.ellipse([P(48.5, 26), P(55, 33)], fill="#6d6a63", outline="#2c2a26", width=2 * SS)
od.ellipse([M(48.5, 26), M(55, 33)], fill=1)
etichetta(64, 16.5, "Tempio", 18)
edificio(140, 122, 9, 7, 18, "#5e4029", porta=(0, -3.5)); etichetta(140, 131, "Casa dell'eroe", 16)
# cimitero
d.rectangle([P(20, 12), P(42, 32)], outline="#5a4a38", width=2 * SS)
od.rectangle([M(20, 12), M(42, 32)], fill=1)
for i in range(4):
    for j in range(3):
        tx, ty = 23.5 + i * 5, 16 + j * 6
        d.rectangle([P(tx, ty), P(tx + 1.4, ty + 2.4)], fill="#8c8a84", outline="#3a3833", width=SS)
etichetta(31, 8.5, "Cimitero", 16)
# molo
my = 97; mx0 = riva(my) - 3
od.rectangle([M(mx0 - 4, my - 3), M(mx0 + 1, my + 3)], fill=1)

# --- case lungo le strade: fitte vicino alla piazza, sempre più rade verso fuori
def densita(x, y):
    dist = math.hypot(x - cx, y - cy)
    if dist < 36: return 1.0
    return max(0.03, 1 - (dist - 36) / 40)
def lungo(pts, w_strada):
    for (ax, ay), (bx, by) in zip(pts, pts[1:]):
        L = math.hypot(bx - ax, by - ay)
        if L < 4: continue
        ux, uy = (bx - ax) / L, (by - ay) / L
        nx, ny = -uy, ux
        ang = math.degrees(math.atan2(uy, ux))
        for lato in (-1, 1):
            t = random.uniform(0.5, 2)
            while t < L - 3:
                w = random.uniform(4.5, 7.5); h = random.uniform(5.5, 8)
                off = w_strada / 2 + 0.4 + h / 2
                x = ax + ux * (t + w / 2) + nx * off * lato
                y = ay + uy * (t + w / 2) + ny * off * lato
                dens = densita(x, y)
                if random.random() < 0.06 * (2 - dens): t += random.uniform(2, 3); continue   # vicoletto
                if random.random() > dens: t += w + random.uniform(2, 8) * (1 - dens); continue
                c = angoli(x, y, w, h, ang)
                if all(0 < px < W_M and 0 < py < H_M for px, py in c) and libero_poly(angoli(x, y, w - 1.2, h - 1.2, ang)):
                    edificio(x, y, w, h, ang, porta=(random.uniform(-w/4, w/4), -lato * h / 2))
                    if random.random() < 0.8 * dens * dens:
                        h2 = random.uniform(5, 7)
                        off2 = off + h / 2 + 0.3 + h2 / 2
                        x2 = ax + ux * (t + w / 2) + nx * off2 * lato
                        y2 = ay + uy * (t + w / 2) + ny * off2 * lato
                        c2 = angoli(x2, y2, w, h2, ang)
                        if all(0 < px < W_M and 0 < py < H_M for px, py in c2) and libero_poly(angoli(x2, y2, w - 1.2, h2 - 1.2, ang)):
                            edificio(x2, y2, w, h2, ang, porta=(0, lato * h2 / 2))
                t += w - 0.2 if dens > 0.8 else w + random.uniform(1, 6) * (1 - dens)

for s in principali[:3]: lungo(s, 3.4)
lungo(principali[3][:3], 3.4)
for s in vicoli: lungo(s, 2.2)
n_case = len(DISEGNI) - 6

# orti dietro le case, fuori dal centro
for (ox, oy, ow, oh) in [(22, 56, 12, 7), (30, 128, 10, 6), (140, 50, 10, 7), (108, 140, 12, 6), (50, 146, 10, 6)]:
    c = [(ox, oy), (ox + ow, oy), (ox + ow, oy + oh), (ox, oy + oh)]
    if libero_poly(c):
        d.rectangle([P(ox, oy), P(ox + ow, oy + oh)], fill="#b9b48c", outline="#6e5b44", width=SS)
        for k in range(1, int(oh)):
            d.line([P(ox + .5, oy + k), P(ox + ow - .5, oy + k)], fill="#8d8763", width=SS)
        od.rectangle([M(ox, oy), M(ox + ow, oy + oh)], fill=1)

# alberi: bosco ai bordi, pochi nel borgo
occ_np = np.array(occ, bool)
def libero_punto(x, y, r):
    x0, x1 = int((x - r) * R), int((x + r) * R); y0, y1 = int((y - r) * R), int((y + r) * R)
    x0, y0 = max(0, x0), max(0, y0)
    return not occ_np[y0:y1, x0:x1].any()
alberi = []
for _ in range(3200):
    x, y = random.uniform(-2, W_M), random.uniform(-2, H_M + 2)
    if x > riva(max(0, min(H_M, y))) - 4: continue
    dist = math.hypot(x - cx, (y - cy) * 1.1)
    p = 0.006 if dist < 50 else (0.12 if dist < 68 else 0.75)
    rr = random.uniform(2.2, 3.6) if dist > 55 else random.uniform(1.6, 2.4)
    if random.random() < p and libero_punto(x, y, rr * 0.8):
        alberi.append((x, y, rr))
alberi.sort(key=lambda a: a[1])

disegna_edifici()
for x, y, r in alberi:
    d.ellipse([P(x - r + .6, y - r + .9), P(x + r + .6, y + r + .9)], fill="#a3a68e")
    d.ellipse([P(x - r, y - r), P(x + r, y + r)], fill=random.choice(["#5d6c4c", "#55654a", "#667455", "#4e5d44"]), outline="#33402c", width=SS)
    d.ellipse([P(x - r * .45, y - r * .55), P(x + r * .1, y - r * .05)], fill="#74825f")

# molo e barche (sopra a tutto)
d.rectangle([P(mx0, my - 1.6), P(mx0 + 30, my + 1.6)], fill="#8a6c4a", outline="#3d2f22", width=2 * SS)
for k in range(int(mx0) + 1, int(mx0 + 30)):
    d.line([P(k, my - 1.6), P(k, my + 1.6)], fill="#6e5538", width=SS)
for k in range(0, 31, 6):
    for s_ in (-1, 1):
        px, py = P(mx0 + k, my + s_ * 1.9)
        d.ellipse([px - .5 * S, py - .5 * S, px + .5 * S, py + .5 * S], fill="#3d2f22")
def barca(x, y, l=5, w=1.8, ang=0):
    a = math.radians(ang); ca, sa = math.cos(a), math.sin(a); pts = []
    for i in range(24):
        t = i / 24 * 2 * math.pi
        dx, dy = l / 2 * math.cos(t), w / 2 * math.sin(t) * (1 - 0.3 * math.cos(t))
        pts.append(P(x + dx * ca - dy * sa, y + dx * sa + dy * ca))
    d.polygon(pts, fill="#9b7a54", outline="#2e2418", width=2 * SS)
barca(mx0 + 10, my + 4.2, ang=3); barca(mx0 + 20, my - 4.4, ang=-4); barca(mx0 + 27, my + 4.6, l=4.4, ang=8)
etichetta(mx0 + 16, my - 9, "Molo", 18, True, "#e9e4d6")

# griglia leggera
img = img.convert("RGBA")
ov = Image.new("RGBA", img.size, (0, 0, 0, 0)); ovd = ImageDraw.Draw(ov)
for gx in range(0, W_M + 1, 10): ovd.line([P(gx, 0), P(gx, H_M)], fill=(255, 255, 255, 110), width=SS)
for gy in range(0, H_M + 1, 10): ovd.line([P(0, gy), P(W_M, gy)], fill=(255, 255, 255, 110), width=SS)
img = Image.alpha_composite(img, ov).convert("RGB")

# foglio
foglio = Image.new("RGB", (IW, IH), "#efe9da"); foglio.paste(img, (MX, MY))
img = foglio; d = ImageDraw.Draw(img)
def Q(x, y): return (MX + x * S, MY + y * S)
for gx in range(0, W_M + 1, 20):
    px, py = Q(gx, 0); d.text((px, py - 8 * SS), f"{gx}", font=f(11), fill="#6b6150", anchor="mb")
for gy in range(0, H_M + 1, 20):
    px, py = Q(0, gy); d.text((px - 8 * SS, py), f"{gy}", font=f(11), fill="#6b6150", anchor="rm")
d.rectangle([Q(0, 0), Q(W_M, H_M)], outline="#3a3125", width=4 * SS)

def testo(x, y, t, sz, b, col):
    alone = "#1f242b" if col.startswith("#e") else "#efe9da"
    d.text(Q(x, y), t, font=f(sz, b), fill=col, anchor="mm", stroke_width=3 * SS, stroke_fill=alone)
for e in etichette: testo(*e)
testo(190, 55, "LAGO NERO", 30, True, "#e9e4d6")
testo(cx, cy + 7, "Piazzetta", 15, True, "#2b241c")
testo(12, 104, "Dal bosco", 14, True, "#2b241c"); testo(12, 117, "(da qui arrivano gli orchi)", 11, False, "#2b241c")
testo(96, 163, "Verso le grotte", 14, True, "#2b241c"); testo(96, 167.5, "(uscita dalla gattabuia)", 11, False, "#2b241c")
testo(14, 70, "Bosco", 18, True, "#2b241c")

d.text((MX, 40 * SS), "Villaggio Lago Nero", font=f(46, True), fill="#2b241c")
d.text((MX, 100 * SS), f"Vista dall'alto, bozza 3: il borgo ({n_case} case). Griglia: un quadrato = 10 metri.", font=f(18), fill="#5a4f40")
nx, ny = MX + W_M * S - 30 * SS, 80 * SS
d.polygon([(nx, ny - 34 * SS), (nx - 12 * SS, ny + 10 * SS), (nx, ny), (nx + 12 * SS, ny + 10 * SS)], fill="#2b241c")
d.text((nx, ny - 52 * SS), "N", font=f(18, True), fill="#2b241c", anchor="mm")

by = MY + H_M * S + 50 * SS
d.text((MX, by), "Scala:", font=f(16, True), fill="#2b241c", anchor="lm")
sx = MX + 80 * SS
for i in range(5):
    d.rectangle([sx + i * 10 * S, by - 6 * SS, sx + (i + 1) * 10 * S, by + 6 * SS], fill="#2b241c" if i % 2 == 0 else "#efe9da", outline="#2b241c", width=SS)
d.text((sx, by + 22 * SS), "0", font=f(12), fill="#2b241c", anchor="mm")
d.text((sx + 50 * S, by + 22 * SS), "50 m", font=f(12), fill="#2b241c", anchor="mm")
lx = sx + 50 * S + 140 * SS
for i, (c, t) in enumerate([("#8b6a4b", "Case"), ("#5e4029", "Edifici importanti"), ("#bca98a", "Strade"),
                            ("#5d6c4c", "Alberi"), ("#1f242b", "Lago"), ("#b9b48c", "Orti"), ("#e7c98a", "Porte")]):
    xx = lx + i * 175 * SS
    d.rectangle([xx, by - 10 * SS, xx + 24 * SS, by + 10 * SS], fill=c, outline="#2b241c", width=SS)
    d.text((xx + 34 * SS, by), t, font=f(15), fill="#2b241c", anchor="lm")

out = img.resize((IW // SS, IH // SS), Image.LANCZOS)
out.save("villaggio-lago-nero.png", optimize=True)

print(out.size, n_case)
