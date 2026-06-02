"""Generate figures for the course-work report: class diagram, flowcharts,
navigation map, architecture stack, and captioned screenshot placeholders.

Output: report_build/figures/*.png
"""
import os
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, "..", "report_build", "figures"))
os.makedirs(OUT, exist_ok=True)

BLUE = (0x2D, 0x6B, 0xD6)
DARKBLUE = (0x1E, 0x4F, 0xA8)
LIGHT = (0xEA, 0xEE, 0xF5)
INK = (0x1F, 0x29, 0x37)
GREY = (0x6B, 0x72, 0x80)
GREEN = (0x22, 0xA0, 0x5A)
RED = (0xC0, 0x39, 0x2B)
WHITE = (255, 255, 255)
SS = 2  # supersample


def F(size, bold=False):
    name = "arialbd.ttf" if bold else "arial.ttf"
    try:
        return ImageFont.truetype("C:/Windows/Fonts/" + name, size * SS)
    except OSError:
        return ImageFont.load_default()


def canvas(w, h, bg=WHITE):
    img = Image.new("RGB", (w * SS, h * SS), bg)
    return img, ImageDraw.Draw(img)


def finish(img, name):
    w, h = img.size
    img = img.resize((w // SS, h // SS), Image.LANCZOS)
    p = os.path.join(OUT, name)
    img.save(p)
    print("wrote", os.path.basename(p))


def tsize(d, text, font):
    b = d.textbbox((0, 0), text, font=font)
    return b[2] - b[0], b[3] - b[1]


def center(d, cx, cy, text, font, fill=INK):
    w, h = tsize(d, text, font)
    d.text((cx - w / 2, cy - h / 2), text, font=font, fill=fill)


def arrow(d, p1, p2, fill=GREY, width=2, head=10):
    import math
    d.line([p1, p2], fill=fill, width=width * SS)
    ang = math.atan2(p2[1] - p1[1], p2[0] - p1[0])
    h = head * SS
    for s in (-0.4, 0.4):
        d.line([p2, (p2[0] - h * math.cos(ang - s), p2[1] - h * math.sin(ang - s))],
               fill=fill, width=width * SS)


# ---------------------------------------------------------------- class diagram
def class_box(d, x, y, w, title, stereo, fields, methods):
    pad = 8 * SS
    lh = 22 * SS
    th = 30 * SS
    body = fields + ["—"] + methods
    h = th + pad + len(body) * lh + pad
    x *= SS; y *= SS; w *= SS
    d.rectangle([x, y, x + w, y + h], fill=WHITE, outline=BLUE, width=2 * SS)
    d.rectangle([x, y, x + w, y + th], fill=BLUE)
    f_t = F(12, True); f_s = F(9); f_b = F(10)
    cy = y + th / 2
    if stereo:
        center(d, x + w / 2, cy - 8 * SS, stereo, f_s, WHITE)
        center(d, x + w / 2, cy + 6 * SS, title, f_t, WHITE)
    else:
        center(d, x + w / 2, cy, title, f_t, WHITE)
    yy = y + th + pad
    for line in body:
        if line == "—":
            d.line([x, yy + lh / 2, x + w, yy + lh / 2], fill=(0xC8, 0xD0, 0xDA), width=1 * SS)
        else:
            d.text((x + pad, yy + 3 * SS), line, font=f_b, fill=INK)
        yy += lh
    return (x, y, x + w, y + h)


def inherit_arrow(d, child_top, parent_bottom):
    # hollow triangle pointing up to parent
    x1, y1 = child_top
    x2, y2 = parent_bottom
    d.line([(x1, y1), (x2, y2 + 12 * SS)], fill=GREY, width=2 * SS)
    s = 9 * SS
    d.polygon([(x2, y2), (x2 - s, y2 + 13 * SS), (x2 + s, y2 + 13 * SS)],
              outline=GREY, fill=WHITE, width=2 * SS)


def assoc(d, p1, p2, label, m1, m2):
    d.line([(p1[0] * SS, p1[1] * SS), (p2[0] * SS, p2[1] * SS)], fill=GREY, width=2 * SS)
    f = F(9)
    d.text((p1[0] * SS + 4 * SS, p1[1] * SS - 16 * SS), m1, font=f, fill=GREY)
    d.text((p2[0] * SS - 18 * SS, p2[1] * SS - 16 * SS), m2, font=f, fill=GREY)
    mx, my = (p1[0] + p2[0]) / 2 * SS, (p1[1] + p2[1]) / 2 * SS
    w, _ = tsize(d, label, f)
    d.rectangle([mx - w / 2 - 3 * SS, my - 9 * SS, mx + w / 2 + 3 * SS, my + 9 * SS], fill=WHITE)
    center(d, mx, my, label, f, GREY)


def class_diagram():
    img, d = canvas(1180, 820, WHITE)
    person = class_box(d, 430, 30, 320, "Person", "«abstract»",
                       ["PersonId : int", "FullName : string", "Phone : string",
                        "Email : string", "Role : PersonRole", "IsActive : bool"],
                       ["+ Validate() : List<string>"])
    attendee = class_box(d, 120, 340, 270, "Attendee", "",
                         ["Notes : string"], ["Attendee()"])
    speaker = class_box(d, 800, 340, 290, "Speaker", "",
                        ["Topic : string", "Bio : string"],
                        ["+ Validate() : List<string>"])
    event = class_box(d, 40, 560, 300, "Event", "",
                      ["EventId : int", "Title : string", "StartAt : DateTime",
                       "Location : string", "Capacity : int", "Status : EventStatus"],
                      ["+ Validate()"])
    reg = class_box(d, 440, 470, 320, "Registration", "",
                    ["RegistrationId : int", "EventId : int", "PersonId : int",
                     "Price : decimal {priv set}", "PaidAmount : decimal {priv set}",
                     "Status : PaymentStatus", "CheckedInAt : DateTime?"],
                    ["+ UpdatePayment()", "+ Cancel()  + CheckIn()",
                     "+ ComputeStatus() : PaymentStatus"])
    expense = class_box(d, 860, 590, 280, "Expense", "",
                        ["ExpenseId : int", "EventId : int", "Category : string",
                         "Amount : decimal", "PaidAt : DateTime?"],
                        ["+ Validate()"])
    # inheritance
    inherit_arrow(d, (attendee[2] - 60 * SS, attendee[1]), (person[0] + 90 * SS, person[3]))
    inherit_arrow(d, (speaker[0] + 60 * SS, speaker[1]), (person[2] - 90 * SS, person[3]))
    # associations (use box edge midpoints, unscaled coords)
    assoc(d, (340, 660), (440, 620), "has", "1", "0..*")          # Event - Registration
    assoc(d, (590, 300), (600, 470), "registers", "1", "0..*")    # Person - Registration
    assoc(d, (340, 795), (860, 760), "incurs", "1", "0..*")       # Event - Expense (below Registration)
    finish(img, "class_diagram.png")


# ------------------------------------------------------------------- flowcharts
def term(d, cx, cy, w, h, text, fill=LIGHT):
    x0, y0, x1, y1 = (cx - w / 2) * SS, (cy - h / 2) * SS, (cx + w / 2) * SS, (cy + h / 2) * SS
    d.rounded_rectangle([x0, y0, x1, y1], radius=h / 2 * SS, fill=fill, outline=BLUE, width=2 * SS)
    center(d, cx * SS, cy * SS, text, F(10, True))


def proc(d, cx, cy, w, h, text, fill=WHITE):
    x0, y0, x1, y1 = (cx - w / 2) * SS, (cy - h / 2) * SS, (cx + w / 2) * SS, (cy + h / 2) * SS
    d.rectangle([x0, y0, x1, y1], fill=fill, outline=INK, width=2 * SS)
    for i, ln in enumerate(text.split("\n")):
        center(d, cx * SS, (cy - (len(text.split("\n")) - 1) * 8 + i * 16) * SS, ln, F(9))


def decision(d, cx, cy, w, h, text):
    pts = [(cx * SS, (cy - h / 2) * SS), ((cx + w / 2) * SS, cy * SS),
           (cx * SS, (cy + h / 2) * SS), ((cx - w / 2) * SS, cy * SS)]
    d.polygon(pts, fill=(0xFF, 0xF4, 0xD6), outline=(0xB9, 0x8A, 0x2E), width=2 * SS)
    for i, ln in enumerate(text.split("\n")):
        center(d, cx * SS, (cy - (len(text.split("\n")) - 1) * 7 + i * 15) * SS, ln, F(9, True))


def vline(d, x, y1, y2, lbl=None):
    arrow(d, (x * SS, y1 * SS), (x * SS, y2 * SS))
    if lbl:
        d.text((x * SS + 5 * SS, ((y1 + y2) / 2 - 8) * SS), lbl, font=F(9, True), fill=GREY)


def hline(d, x1, x2, y, lbl=None):
    arrow(d, (x1 * SS, y * SS), (x2 * SS, y * SS))
    if lbl:
        d.text(((x1 + 8) * SS, (y - 16) * SS), lbl, font=F(9, True), fill=GREY)


def flow_computestatus():
    img, d = canvas(780, 730)
    cx = 250
    term(d, cx, 30, 150, 40, "Start")
    steps = [
        ("cancelled?", "Status = Cancelled"),
        ("price ≤ 0.005?", "Status = Free"),
        ("paid ≤ 0.005?", "Status = Unpaid"),
        ("paid + 0.005\n< price?", "Status = PartPaid"),
    ]
    y = 110
    vline(d, cx, 50, y - 35)
    for q, res in steps:
        decision(d, cx, y, 150, 80, q)
        # yes branch to the right -> sets status and returns (terminal outcome)
        hline(d, cx + 75, 520, y, "yes")
        proc(d, 630, y, 190, 46, res, (0xDD, 0xF1, 0xE3))
        # no branch down
        ny = y + 130
        vline(d, cx, y + 40, ny - 35, "no")
        y = ny
    proc(d, cx, y, 180, 46, "Status = Paid", (0xDD, 0xF1, 0xE3))
    vline(d, cx, y + 23, y + 58)
    term(d, cx, y + 88, 180, 42, "Return Status")
    d.text((470 * SS, (y + 80) * SS),
           "Each outcome box assigns the status that the method then returns.",
           font=F(9), fill=GREY)
    finish(img, "flow_computestatus.png")


def flow_createregistration():
    img, d = canvas(820, 700)
    cx = 250
    term(d, cx, 30, 170, 40, "Start")
    vline(d, cx, 50, 75)
    decision(d, cx, 115, 180, 90, "activeCount ≥\nCapacity?")
    hline(d, cx + 90, 560, 115, "yes")
    proc(d, 640, 115, 230, 55, "throw\nInvalidOperationException\n(capacity reached)", (0xFB, 0xE4, 0xE1))
    vline(d, cx, 160, 215, "no")
    decision(d, cx, 265, 180, 90, "active duplicate\nexists?")
    hline(d, cx + 90, 560, 265, "yes")
    proc(d, 640, 265, 230, 55, "throw\nInvalidOperationException\n(already registered)", (0xFB, 0xE4, 0xE1))
    vline(d, cx, 310, 360, "no")
    proc(d, cx, 390, 200, 55, "build Registration;\nUpdatePayment(price, paid)")
    vline(d, cx, 418, 455)
    decision(d, cx, 500, 180, 80, "Validate()\nerrors?")
    hline(d, cx + 90, 560, 500, "yes")
    proc(d, 640, 500, 230, 50, "throw\nArgumentException", (0xFB, 0xE4, 0xE1))
    vline(d, cx, 540, 590, "no")
    term(d, cx, 620, 200, 42, "return Registration", (0xDD, 0xF1, 0xE3))
    finish(img, "flow_createregistration.png")


# ------------------------------------------------------------------ navigation
def nav_diagram():
    img, d = canvas(900, 470)
    proc(d, 150, 235, 180, 70, "MainWindow\n(sidebar nav)", LIGHT)
    pages = ["Events", "Registrations", "People", "Check-in", "Finance", "Reports"]
    y = 40
    for p in pages:
        proc(d, 540, y, 200, 46, p)
        arrow(d, (240 * SS, 235 * SS), (440 * SS, (y) * SS))
        y += 76
    proc(d, 820, 40, 130, 46, "Event details", (0xFF, 0xF4, 0xD6))
    arrow(d, (640 * SS, 40 * SS), (755 * SS, 40 * SS))
    d.text((648 * SS, 18 * SS), "double-click", font=F(8), fill=GREY)
    finish(img, "nav_diagram.png")


# ---------------------------------------------------------------- architecture
def architecture():
    img, d = canvas(560, 520)
    layers = [("Views (WPF / XAML)", BLUE), ("ViewModels (MVVM)", (0x3E, 0x7C, 0xE0)),
              ("Services (Finance, Registration, CSV)", (0x52, 0x8A, 0xE6)),
              ("Repositories (IRepository<T>)", (0x6B, 0x9B, 0xEB)),
              ("Data (SQLite connection + schema)", (0x86, 0xAE, 0xF0)),
              ("Domain (entities, enums, IValidatable)", (0xA3, 0xC2, 0xF4))]
    y = 30
    for name, col in layers:
        d.rectangle([60 * SS, y * SS, 500 * SS, (y + 60) * SS], fill=col, outline=WHITE, width=2 * SS)
        center(d, 280 * SS, (y + 30) * SS, name, F(12, True), WHITE)
        if y > 30:
            arrow(d, (280 * SS, (y - 18) * SS), (280 * SS, (y) * SS), fill=GREY)
        y += 78
    d.text((60 * SS, (y + 2) * SS), "Each layer depends only on the layer(s) below it.",
           font=F(10), fill=GREY)
    finish(img, "architecture.png")


# -------------------------------------------------------- screenshot placeholders
def placeholder(name, caption):
    img, d = canvas(900, 520, (0xF4, 0xF6, 0xFA))
    d.rectangle([6 * SS, 6 * SS, 894 * SS, 514 * SS], outline=(0xB6, 0xC2, 0xD4), width=3 * SS)
    center(d, 450 * SS, 220 * SS, "\U0001F5BC", F(60))
    center(d, 450 * SS, 290 * SS, "SCREENSHOT PLACEHOLDER", F(18, True), GREY)
    center(d, 450 * SS, 325 * SS, caption, F(13), INK)
    center(d, 450 * SS, 360 * SS, "Replace with your captured screenshot.", F(11), GREY)
    finish(img, name)


def main():
    class_diagram()
    flow_computestatus()
    flow_createregistration()
    nav_diagram()
    architecture()
    shots = [
        ("shot_events.png", "Events page — list, search and summary cards"),
        ("shot_eventdetails.png", "Event details — registrations and finance tabs"),
        ("shot_registration.png", "Add / edit registration dialog (free-entry toggle)"),
        ("shot_checkin.png", "Check-in page during the event"),
        ("shot_finance.png", "Finance page — income, expenses and profit"),
        ("shot_reports.png", "Reports page and an exported CSV"),
    ]
    for n, c in shots:
        placeholder(n, c)


if __name__ == "__main__":
    main()
