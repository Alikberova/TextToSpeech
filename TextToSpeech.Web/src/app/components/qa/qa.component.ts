import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {MatExpansionModule} from '@angular/material/expansion';
import { QaList } from '../../constants/content/qa';

@Component({
    selector: 'app-qa',
    imports: [CommonModule, MatExpansionModule],
    templateUrl: './qa.component.html',
    styleUrls: ['./qa.component.scss']
})
export class QaComponent {
  qas = QaList;
}