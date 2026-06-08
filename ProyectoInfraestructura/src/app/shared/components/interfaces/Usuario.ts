  export interface Usuario {
    id?: number;
    nombreCompleto?: string;
    permisos?: string | null;
    correo: string;
    password: string;
    imagenUrl?: string | null;
    permisosJson?: { [key: string]: string };
    permisosCategoria?: string[];
  }
