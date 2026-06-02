"""Generate the EventPlanner logo: a calendar mark with a check.

Outputs:
  Source/EventPlanner/logo.png   - 512x512 app logo (used in the WPF UI)
  Source/EventPlanner/app.ico    - multi-size icon (used as the exe icon)
"""
import os
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.normpath(os.path.join(HERE, "..", "Source", "EventPlanner"))

S = 512
SCALE = 4  # supersample for smooth edges


def rounded_mask(size, radius):
    m = Image.new("L", (size, size), 0)
    d = ImageDraw.Draw(m)
    d.rounded_rectangle([0, 0, size - 1, size - 1], radius=radius, fill=255)
    return m


def vgradient(size, top, bottom):
    g = Image.new("RGB", (1, size))
    for y in range(size):
        t = y / (size - 1)
        g.putpixel((0, y), tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)))
    return g.resize((size, size))


def build(size):
    n = size * SCALE
    img = Image.new("RGBA", (n, n), (0, 0, 0, 0))

    # Background: blue gradient clipped to a rounded square.
    bg = vgradient(n, (0x4F, 0x8E, 0xF7), (0x2D, 0x6B, 0xD6)).convert("RGBA")
    img.paste(bg, (0, 0), rounded_mask(n, int(n * 0.22)))

    d = ImageDraw.Draw(img)

    # Calendar body (white rounded rect).
    m = n * 0.16
    top = n * 0.30
    body = [m, top, n - m, n - n * 0.16]
    d.rounded_rectangle(body, radius=int(n * 0.06), fill=(255, 255, 255, 255))

    # Header band of the calendar (deeper blue) across the top of the body.
    band_h = (body[3] - body[1]) * 0.22
    band = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    bd = ImageDraw.Draw(band)
    bd.rounded_rectangle([body[0], body[1], body[2], body[1] + band_h + n * 0.06],
                         radius=int(n * 0.06), fill=(0x1E, 0x4F, 0xA8, 255))
    bd.rectangle([body[0], body[1] + band_h, body[2], body[1] + band_h + n * 0.06],
                 fill=(0, 0, 0, 0))
    # keep only the top rounded part of the band over the body
    img.alpha_composite(Image.composite(band, Image.new("RGBA", (n, n), (0, 0, 0, 0)),
                                         rounded_mask_rect(n, body, int(n * 0.06))))

    # Binder rings.
    ring_w = n * 0.035
    for cx in (m + (body[2] - body[0]) * 0.30, m + (body[2] - body[0]) * 0.70):
        d.rounded_rectangle([cx - ring_w, top - n * 0.06, cx + ring_w, top + n * 0.05],
                            radius=int(ring_w), fill=(0xE8, 0xEE, 0xF6, 255))

    # Green check in the calendar body.
    cx0 = m + (body[2] - body[0]) * 0.24
    cy0 = body[1] + band_h + (body[3] - (body[1] + band_h)) * 0.58
    p1 = (cx0, cy0)
    p2 = (cx0 + (body[2] - body[0]) * 0.16, cy0 + (body[3] - (body[1] + band_h)) * 0.22)
    p3 = (cx0 + (body[2] - body[0]) * 0.46, cy0 - (body[3] - (body[1] + band_h)) * 0.30)
    d.line([p1, p2, p3], fill=(0x22, 0xA0, 0x5A, 255), width=int(n * 0.045), joint="curve")

    return img.resize((size, size), Image.LANCZOS)


def rounded_mask_rect(size, box, radius):
    m = Image.new("L", (size, size), 0)
    ImageDraw.Draw(m).rounded_rectangle(box, radius=radius, fill=255)
    return m


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    big = build(S)
    png_path = os.path.join(OUT_DIR, "logo.png")
    big.save(png_path)
    print("wrote", png_path)

    ico_path = os.path.join(OUT_DIR, "app.ico")
    big.save(ico_path, sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64),
                              (128, 128), (256, 256)])
    print("wrote", ico_path)


if __name__ == "__main__":
    main()
