import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ContractDto {
  id: string;
  projectId: string;
  contractTemplateId: string;
  contractTitle: string;
  contractType: string;
  partyName: string;
  partyPhone: string;
  partyNationalId: string;
  contractValue: number | null;
  startDate: string | null;
  endDate: string | null;
  status: string;
  contractDataJson: string | null;
  pdfFileId: string | null;
  signedFileId?: string | null;
  templateNameAr: string | null;
  templateNameEn: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class ReadyContractsService {
  constructor(private http: HttpClient) {}

  /** List all contracts for a project. */
  getProjectContracts(projectId: string): Observable<ContractDto[]> {
    return this.http.get<ContractDto[]>(
      `${environment.apiUrl}/projects/${projectId}/contracts`
    );
  }

  /** Get a single contract by ID. */
  getProjectContract(projectId: string, contractId: string): Observable<ContractDto> {
    return this.http.get<ContractDto>(
      `${environment.apiUrl}/projects/${projectId}/contracts/${contractId}`
    );
  }

  /** Download contract PDF as Blob — uses Authorization header via interceptor, never JWT in URL. */
  downloadContractPdf(projectId: string, pdfFileId: string): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}/projects/${projectId}/files/${pdfFileId}/download`,
      { responseType: 'blob' }
    );
  }

  /** Download signed contract PDF as Blob — uses Authorization header via interceptor, never JWT in URL. */
  downloadSignedContractPdf(projectId: string, signedFileId: string): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}/projects/${projectId}/files/${signedFileId}/download`,
      { responseType: 'blob' }
    );
  }
}
