import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { Observable } from 'rxjs';

interface PolicySection {
  heading?: string;
  paragraphs: string[];
}

@Component({
  selector: 'app-privacy-policy',
  standalone: true,
  imports: [ CommonModule, MatCardModule ],
  templateUrl: './privacy-policy.component.html',
  styleUrl: './privacy-policy.component.scss'
})

export class PrivacyPolicyComponent implements OnInit {
  private policyUrl = '../../../assets/privacy-policy.txt';
  policySections: PolicySection[] = [];
  lastUpdatedDate = 'TODO: Date of production launch';

  constructor(private http: HttpClient) {}
  ngOnInit() {
    this.getPrivacyPolicy().subscribe(data => {
      const sections = data.split('\r\n').filter(section => section.trim() !== '');
      sections.forEach(section => {
        const lines = section.split('\n');
        const sectionObj: PolicySection = { paragraphs: [] };

        lines.forEach((line, index) => {
          if (index === 0 && line.startsWith('#')) {
            sectionObj.heading = line.substring(1).trim(); // Assume first line could be a heading
          } else {
            sectionObj.paragraphs.push(line);
          }
        });

        this.policySections.push(sectionObj);
      });
    });
    console.log(this.policySections);
  }

  getPrivacyPolicy(): Observable<string> {
    return this.http.get(this.policyUrl, { responseType: 'text' });
  }
}
