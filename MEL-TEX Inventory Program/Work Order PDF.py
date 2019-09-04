import string
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.lib.pagesizes import letter
import sys
import csv
from reportlab.platypus import Table, TableStyle, SimpleDocTemplate, Paragraph, Spacer
from os import startfile, linesep

stylesheet = getSampleStyleSheet()

# grab the data from the file
paragraph = ""
headers = []
rows = []
with open(sys.argv[1]) as csv_file:
    reader = csv.reader(csv_file, delimiter=';')
    # noinspection PyRedeclaration
    rows = list(reader)
    # noinspection PyRedeclaration
    headers = rows.pop(0)

    # add each element as a string to write as a paragraph later
    for row in rows:
        paragraph += ''.join(row)

# grab the date, quote/sales order/presale number, and shipping/billing info from the file
date = headers.pop(0)
salesorderNum = headers.pop(0)
quoteNum = headers.pop(0)
buyer = str(headers.pop(0)).replace("\\n", "\n")
shipTo = str(headers.pop(0)).replace("\\n", "\n")
billTo = str(headers.pop(0)).replace("\\n", "\n")
shipvia = headers.pop(0)
terms = headers.pop(0)
fob = headers.pop(0)
freightTerms = headers.pop(0)
repNum = headers.pop(0)
repName = headers.pop(0)

# grab the width and height of the page and set the document style
width, height = letter
doc = SimpleDocTemplate(sys.argv[2], pagesize=letter)

# setup and add the header (date  MEL-TEX  Quote #)
header = [["MEL-TEX", "Work Order", salesorderNum + "\n\n" + quoteNum + "\n\nDate: " + date]]
t = Table(header, colWidths=width * 0.33, rowHeights=40, spaceBefore=0)
t.setStyle(TableStyle([('ALIGN', (0, 0), (-1, -1), 'CENTER'),
                       ('VALIGN', (0, 0), (-1, -1), 'TOP'),
                       ('FONTSIZE', (2, 0), (2, 0), 12),
                       ('FONTSIZE', (0, 0), (0, 0), 25),
                       ('FONTSIZE', (1, 0), (1, 0), 35),
                       ('LEFTPADDING', (0, 0), (-1, -1), 8),
                       ('RIGHTPADDING', (0, 0), (-1, -1), 8)]))

# setup and add the shipping/billine items
info = [["Buyer:", "Ship To:", "Bill To:"],
        [buyer, shipTo, billTo]]
t1 = Table(info, colWidths=width * 0.33, rowHeights=80, spaceBefore=40)
t1.setStyle(TableStyle([('ALIGN', (0, 0), (-1, 1), 'LEFT'),
                        ('LEADING', (0, 0), (-1, -1), 18*1.2),
                        ('TEXTCOLOR', (0, 0), (-1, -1), colors.black),
                        ('VALIGN', (0, 0), (-1, -1), 'BOTTOM'),
                        ('VALIGN', (0, 1), (-1, -1), 'TOP'),
                        ('FONTSIZE', (0, 0), (-1, 0), 11),
                        ('LEFTPADDING', (0, 0), (-1, 0), 20),
                        ('LEFTPADDING', (0, 1), (-1, 1), 40),
                        ('FONTSIZE', (0, 1), (-1, 1), 13)]))

shipping = [["Ship Via:", "Terms:", "FOB:", "Freight Terms:", "Rep Number:", "Rep Name:"],
            [shipvia, terms, fob, freightTerms, repNum, repName]]
t2 = Table(shipping, colWidths=width * .16, rowHeights=30, spaceBefore=20)
t2.setStyle(TableStyle([('ALIGN', (0, 0), (-1, -1), 'LEFT'),
                        ('LEADING', (0, 0), (-1, -1), 15*1.2),
                        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
                        ('FONTSIZE', (0, 0), (-1, -1), 11),
                        ('INNERGRID', (0, 0), (-1, -1), 0.25, colors.black),
                        ('BOX', (0, 0), (-1, -1), 0.25, colors.black)]))

# separate the paragraph from the table above
paragraph = "<br/><br/><br/><br/><br/><br/>" + paragraph

# add all the stuff to the page
elements = [t, t1, t2, Paragraph(paragraph, stylesheet['Normal'])]

# build and save the PDF
doc.build(elements)

# show the file for printing
if len(sys.argv) == 4:
    startfile(sys.argv[2])
