import { Injectable } from '@angular/core';
import writeXlsxFile from 'write-excel-file/browser';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

export interface ColDef {
  key: string;
  header: string;
}

@Injectable({ providedIn: 'root' })
export class ExportService {

  toExcel(data: any[], cols: ColDef[], filename: string): void {
    const header = cols.map(c => c.header);
    const rows = data.map(row =>
      cols.map(c => {
        const val = row[c.key];
        return Array.isArray(val) ? val.join(', ') : (val ?? '');
      })
    );

    const maxWidths = header.map((h, i) =>
      Math.max(h.length, ...rows.map(r => String(r[i] ?? '').length))
    );

    const headerRow = header.map(h => ({ value: h, fontWeight: 'bold' as const }));
    const dataRows = rows.map(row => row.map(cell => ({ value: String(cell) })));
    const columns = maxWidths.map(w => ({ width: Math.min(w + 2, 50) }));

    writeXlsxFile([headerRow, ...dataRows], { columns })
      .toFile(`${filename}.xlsx`)
      .catch(console.error);
  }

  toPdf(data: any[], cols: ColDef[], filename: string, titulo: string): void {
    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });

    doc.setFontSize(13);
    doc.text(titulo, 14, 15);
    doc.setFontSize(9);
    doc.setTextColor(120);
    doc.text(`Generado: ${new Date().toLocaleString('es-MX')}`, 14, 22);

    const head = [cols.map(c => c.header)];
    const body = data.map(row =>
      cols.map(c => {
        const val = row[c.key];
        return Array.isArray(val) ? val.join(', ') : String(val ?? '');
      })
    );

    autoTable(doc, {
      head,
      body,
      startY: 27,
      styles:       { fontSize: 7, cellPadding: 2, overflow: 'linebreak' },
      headStyles:   { fillColor: [30, 58, 138], textColor: 255, fontStyle: 'bold' },
      alternateRowStyles: { fillColor: [240, 244, 255] },
      margin: { left: 14, right: 14 },
    });

    doc.save(`${filename}.pdf`);
  }
}
