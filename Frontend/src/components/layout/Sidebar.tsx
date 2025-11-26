'use client';

import { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  LayoutDashboard,
  Users,
  CreditCard,
  Calendar,
  Activity,
  Settings,
  ChevronDown,
  ChevronRight,
  UserPlus,
  UserCheck,
  ClipboardList,
  Receipt,
  DollarSign,
  CalendarDays,
  CalendarCheck,
  Dumbbell,
  ListChecks,
  UserCog,
  Shield,
  Database,
} from 'lucide-react';

interface MenuItem {
  label: string;
  icon: React.ReactNode;
  href?: string;
  children?: {
    label: string;
    href: string;
    icon: React.ReactNode;
  }[];
}

const menuItems: MenuItem[] = [
  {
    label: 'Dashboard',
    icon: <LayoutDashboard className="w-5 h-5" />,
    href: '/dashboard',
  },
  {
    label: 'Socios',
    icon: <Users className="w-5 h-5" />,
    children: [
      {
        label: 'Listado de Socios',
        href: '/dashboard/socios',
        icon: <ClipboardList className="w-4 h-4" />,
      },
      {
        label: 'Nuevo Socio',
        href: '/dashboard/socios/nuevo',
        icon: <UserPlus className="w-4 h-4" />,
      },
      /* {
        label: 'Socios Activos',
        href: '/dashboard/socios/activos',
        icon: <UserCheck className="w-4 h-4" />,
      }, */
    ],
  },
  {
    label: 'Membresias',
    icon: <CreditCard className="w-5 h-5" />,
    children: [
      {
        label: 'Todas las Membresias',
        href: '/dashboard/membresias',
        icon: <ClipboardList className="w-4 h-4" />,
      },
      {
        label: 'Nueva Membresia',
        href: '/dashboard/membresias/nueva',
        icon: <CreditCard className="w-4 h-4" />,
      },
    ],
  },
  {
    label: 'Pagos',
    icon: <DollarSign className="w-5 h-5" />,
    children: [
      {
        label: 'Historial de Pagos',
        href: '/dashboard/pagos',
        icon: <Receipt className="w-4 h-4" />,
      },
      {
        label: 'Registrar Pago',
        href: '/dashboard/pagos/nuevo',
        icon: <DollarSign className="w-4 h-4" />,
      },
      {
        label: 'Estadísticas',
        href: '/dashboard/pagos/estadisticas',
        icon: <Activity className="w-4 h-4" />,
      },
    ],
  },
  {
    label: 'Asistencias',
    icon: <Calendar className="w-5 h-5" />,
    children: [
      {
        label: 'Registro de Asistencias',
        href: '/dashboard/asistencias',
        icon: <CalendarDays className="w-4 h-4" />,
      },
      {
        label: 'Marcar Asistencia',
        href: '/dashboard/asistencias/marcar',
        icon: <CalendarCheck className="w-4 h-4" />,
      },
    ],
  },
  {
    label: 'Actividades',
    icon: <Activity className="w-5 h-5" />,
    children: [
      {
        label: 'Todas las Actividades',
        href: '/dashboard/actividades',
        icon: <ListChecks className="w-4 h-4" />,
      },
      {
        label: 'Nueva Actividad',
        href: '/dashboard/actividades/nueva',
        icon: <Dumbbell className="w-4 h-4" />,
      },
    ],
  },
  {
    label: 'Configuracion',
    icon: <Settings className="w-5 h-5" />,
    children: [
      {
        label: 'Usuarios',
        href: '/dashboard/usuarios',
        icon: <UserCog className="w-4 h-4" />,
      },
      {
        label: 'Roles',
        href: '/dashboard/configuracion/roles',
        icon: <Shield className="w-4 h-4" />,
      },
      {
        label: 'Sistema',
        href: '/dashboard/configuracion/sistema',
        icon: <Database className="w-4 h-4" />,
      },
    ],
  },
];

interface SidebarProps {
  isCollapsed: boolean;
  onToggle: () => void;
}

export default function Sidebar({ isCollapsed, onToggle }: SidebarProps) {
  const pathname = usePathname();
  const [expandedItems, setExpandedItems] = useState<string[]>(['Socios']);

  const toggleExpand = (label: string) => {
    setExpandedItems((prev) =>
      prev.includes(label)
        ? prev.filter((item) => item !== label)
        : [...prev, label]
    );
  };

  const isActive = (href: string) => pathname === href;
  const isParentActive = (children: { href: string }[]) =>
    children.some((child) => pathname.startsWith(child.href));

  return (
    <aside
      className={`fixed left-0 top-0 h-full bg-gray-900 text-white transition-all duration-300 z-40 ${
        isCollapsed ? 'w-16' : 'w-64'
      }`}
    >
      {/* Logo */}
      <div className="h-16 flex items-center justify-center border-b border-gray-700">
        {isCollapsed ? (
          <span className="text-2xl font-bold text-blue-400">C</span>
        ) : (
          <span className="text-xl font-bold text-blue-400">Club Manager</span>
        )}
      </div>

      {/* Navigation */}
      <nav className="mt-4 px-2">
        <ul className="space-y-1">
          {menuItems.map((item) => (
            <li key={item.label}>
              {item.href ? (
                <Link
                  href={item.href}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors ${
                    isActive(item.href)
                      ? 'bg-blue-600 text-white'
                      : 'text-gray-300 hover:bg-gray-800 hover:text-white'
                  }`}
                >
                  {item.icon}
                  {!isCollapsed && <span>{item.label}</span>}
                </Link>
              ) : (
                <>
                  <button
                    onClick={() => toggleExpand(item.label)}
                    className={`w-full flex items-center justify-between px-3 py-2.5 rounded-lg transition-colors ${
                      item.children && isParentActive(item.children)
                        ? 'bg-gray-800 text-white'
                        : 'text-gray-300 hover:bg-gray-800 hover:text-white'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      {item.icon}
                      {!isCollapsed && <span>{item.label}</span>}
                    </div>
                    {!isCollapsed && item.children && (
                      <span className="text-gray-400">
                        {expandedItems.includes(item.label) ? (
                          <ChevronDown className="w-4 h-4" />
                        ) : (
                          <ChevronRight className="w-4 h-4" />
                        )}
                      </span>
                    )}
                  </button>

                  {/* Submenu */}
                  {!isCollapsed &&
                    item.children &&
                    expandedItems.includes(item.label) && (
                      <ul className="mt-1 ml-4 space-y-1">
                        {item.children.map((child) => (
                          <li key={child.href}>
                            <Link
                              href={child.href}
                              className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors ${
                                isActive(child.href)
                                  ? 'bg-blue-600 text-white'
                                  : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                              }`}
                            >
                              {child.icon}
                              <span>{child.label}</span>
                            </Link>
                          </li>
                        ))}
                      </ul>
                    )}
                </>
              )}
            </li>
          ))}
        </ul>
      </nav>
    </aside>
  );
}
