import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DocumentService } from '../services/document.service';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { DocumentType } from '../constants/route-constants';

@Component({
  selector: 'app-legal-document',
  standalone: true,
  imports: [ CommonModule, MatCardModule ],
  templateUrl: './legal-document.component.html',
  styleUrl: './legal-document.component.scss'
})
export class LegalDocumentComponent {
  document: any;
  lastUpdatedDate: string = '';

  constructor(
    private route: ActivatedRoute,
    private documentService: DocumentService
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const documentType = params.get(DocumentType)!;
      this.document = this.documentService.getDocument(documentType);
      this.lastUpdatedDate = this.document.lastUpdatedDate;
      window.scrollTo(0, 0);
    });
  }
}
