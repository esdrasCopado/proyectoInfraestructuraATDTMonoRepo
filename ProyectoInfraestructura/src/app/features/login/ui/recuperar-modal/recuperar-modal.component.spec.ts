import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RecuperarModalComponent } from './recuperar-modal.component';

describe('RecuperarModalComponent', () => {
  let component: RecuperarModalComponent;
  let fixture: ComponentFixture<RecuperarModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [RecuperarModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RecuperarModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
