import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterOutlet } from "@angular/router";
import { AppNavComponent } from "./components/app-nav/app-nav.component";
import { FooterComponent } from "./components/footer/footer.component";
import { HomeComponent } from "./components/home/home.component";
import { ScrollButtonComponent } from "./components/scroll-button/scroll-button.component";

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [CommonModule, FormsModule, RouterOutlet, AppNavComponent, HomeComponent, FooterComponent, ScrollButtonComponent]
})
export class AppComponent {
}
